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

    public override void ActivateNotification(string notificationId, string? actionId, INotification? notification = null, bool isLaunchedByNotification = false)
    {
        if (_isAppLaunchedByNotification && _lauchEventTrigged is false)
        {
            if (notification is ToastSession toastSession && toastSession.IsCreatedByCurrentProcess is false)
            {
                _lauchEventTrigged = true;
                base.ActivateNotification(notificationId, actionId, notification, true);
                return;
            }
        }

        base.ActivateNotification(notificationId, actionId, notification, isLaunchedByNotification);
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