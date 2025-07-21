#if WINDOWS

using Microsoft.Toolkit.Uwp.Notifications;
using NativeNotification.Common;
using NativeNotification.Interface;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.UI.Notifications;

namespace NativeNotification.Windows;

public partial class WindowsNotificationManager : INotificationManager, IDisposable
{
    private bool _disposed = false;
    private readonly ActionManager<string> _actionManager = new();
    private readonly ToastNotifier Notifer;
    public WindowsNotificationManager()
    {
        Notifer = ToastNotificationManager.CreateToastNotifier(RegistFromCurrentProcess());
        // 向系统注册通知按钮回调，如果不注册，系统会打开新的进程；真正的回调在ToastSession中
        ToastNotificationManagerCompat.OnActivated += (args) => { };
    }

    public INotification Create()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(WindowsNotificationManager));
        return new ToastSession(Notifer, _actionManager);
    }

    public IProgressNotification CreateProgress()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(WindowsNotificationManager));
        return new ProgressSession(Notifer, _actionManager);
    }

    public void RomoveAllNotifications()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(WindowsNotificationManager));
        _actionManager.Reset();
        ToastNotificationManagerCompat.History.Clear();
    }

    [LibraryImport("shell32.dll", SetLastError = true)]
    private static partial void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

    public void UnRegistFromCurrentProcess()
    {
        _actionManager.Reset();
        ToastNotificationManagerCompat.Uninstall();
    }

    public static string RegistFromCurrentProcess(string? customName = null, string? appUserModelId = null)
    {
        var mainModule = Process.GetCurrentProcess().MainModule;

        if (mainModule?.FileName == null)
        {
            throw new InvalidOperationException("No valid process module found.");
        }

        var appName = customName ?? Path.GetFileNameWithoutExtension(mainModule.FileName);
        var aumid = (appUserModelId ?? appName) + "_" + new Guid().ToString();

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

    public void Dispose()
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