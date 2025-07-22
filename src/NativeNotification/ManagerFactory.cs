using NativeNotification.Interface;
using NativeNotification.Linux;
#if MACOS
using NativeNotification.MacOS;
#endif
#if WINDOWS
using NativeNotification.Windows;
#endif

namespace NativeNotification;

public class ManagerFactory
{
    public static INotificationManager GetNotificationManager(NotificationManagerConfig? config = default)
    {
#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            return new WindowsNotificationManager();
        }
#endif
#if MACOS
        if (OperatingSystem.IsMacOS())
        {
            return new NSNotificationManager();
        }
#endif
        if (OperatingSystem.IsLinux())
        {
            return new DBusNotificationManager(config);
        }

        throw new NotSupportedException("Notification manager not supported on this platform.");
    }
}
