#if WINDOWS

using Microsoft.Toolkit.Uwp.Notifications;
using NativeNotification.Common;
using NativeNotification.Interface;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.UI.Notifications;

namespace NativeNotification.Windows;

public partial class WindowsNotificationManager : NotificationManagerBase<string>, IDisposable
{
    private bool _disposed = false;
    public readonly ToastNotifier Center;
    public WindowsNotificationManager(NativeNotificationOption? config = default)
    {
        Center = ToastNotificationManager.CreateToastNotifier(RegistFromCurrentProcess(config?.AppName));
        // 向系统注册通知按钮回调，如果不注册，系统会打开新的进程；真正的回调在ToastSession中
        ToastNotificationManagerCompat.OnActivated += (args) => { };
    }

    public override INotification Create()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(WindowsNotificationManager));
        return new ToastSession(this);
    }

    public override IProgressNotification CreateProgress()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(WindowsNotificationManager));
        return new ProgressSession(this);
    }

    public override void RomoveAllNotifications()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(WindowsNotificationManager));
        SessionHistory.Reset();
        ToastNotificationManagerCompat.History.Clear();
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

    ~WindowsNotificationManager()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        UnRegistFromCurrentProcess();
        GC.SuppressFinalize(this);
    }
}

#endif