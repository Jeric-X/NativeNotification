using Microsoft.Toolkit.Uwp.Notifications;
using NativeNotification.Common;
using NativeNotification.Interface;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.UI.Notifications;

namespace NativeNotification.Windows;

public partial class WindowsNotificationManager : NotificationManagerBase
{
    public readonly ToastNotifier Center;
    private readonly bool _isAppLaunchedByNotification = false;
    public override bool IsAppLaunchedByNotification => _isAppLaunchedByNotification;
    private bool _lauchEventTrigged = false;
    private bool _previousNotificationAdded = false;

    public WindowsNotificationManager(NativeNotificationOption? config = default)
    {
        Center = ToastNotificationManager.CreateToastNotifier(RegistFromCurrentProcess(config?.AppName));
        _isAppLaunchedByNotification = ToastNotificationManagerCompat.WasCurrentProcessToastActivated();
        if (_isAppLaunchedByNotification is false)
        {
            AddPreviousNotification();
        }

        ToastNotificationManagerCompat.OnActivated += (args) =>
        {
            AddPreviousNotification();
        };
    }

    private void AddPreviousNotification()
    {
        if (_previousNotificationAdded)
        {
            return;
        }
        _previousNotificationAdded = true;
        var list = ToastNotificationManagerCompat.History.GetHistory();
        foreach (var toast in list)
        {
            var notification = new ToastSession(this, toast);
            SessionHistory.AddSession(notification.NotificationId, notification);
        }
    }

    public override INotification Create()
    {
        return new ToastSession(this);
    }

    public override IProgressNotification CreateProgress(bool suppressNotSupportedException)
    {
        return new ProgressSession(this);
    }

    public override void RomoveAllNotifications()
    {
        SessionHistory.Reset();
        ToastNotificationManagerCompat.History.Clear();
    }

    private bool IsLaunchedByNotification()
    {
        if (_isAppLaunchedByNotification && _lauchEventTrigged is false)
        {
            return true;
        }
        return false;
    }

    internal override void ActivateContentClicked(string notificationId, INotification? notification = null, bool isLaunchedByNotification = false)
    {
        if (IsLaunchedByNotification())
        {
            base.ActivateContentClicked(notificationId, notification, true);
            _lauchEventTrigged = true;
            return;
        }
        base.ActivateContentClicked(notificationId, notification, isLaunchedByNotification);
    }

    internal override void ActivateButtonClicked(string notificationId, string? actionId, INotification? notification = null, bool isLaunchedByNotification = false)
    {
        if (IsLaunchedByNotification())
        {
            base.ActivateButtonClicked(notificationId, actionId, notification, true);
            _lauchEventTrigged = true;
            return;
        }
        base.ActivateButtonClicked(notificationId, actionId, notification, isLaunchedByNotification);
    }

    [LibraryImport("shell32.dll", SetLastError = true)]
    private static partial void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

    public void UnRegistFromCurrentProcess()
    {
        SessionHistory.Reset();
        ToastNotificationManagerCompat.Uninstall();
    }

    public static string RegistFromCurrentProcess(string? customName = null)
    {
        var mainModule = Process.GetCurrentProcess().MainModule;

        if (mainModule?.FileName == null)
        {
            throw new InvalidOperationException("No valid process module found.");
        }

        var appName = customName ?? Path.GetFileNameWithoutExtension(mainModule.FileName);
        var aumid = appName + "_" + new Guid().ToString();

        SetCurrentProcessExplicitAppUserModelID(aumid);

        using var shortcut = new ShellLink
        {
            TargetPath = mainModule.FileName,
            Arguments = string.Empty,
            AppUserModelID = aumid
        };

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var startMenuPath = Path.Combine(appData, @"Microsoft\Windows\Start Menu\Programs");
        var shortcutFile = Path.Combine(startMenuPath, $"{appName}.lnk");

        shortcut.Save(shortcutFile);
        return aumid;
    }
}