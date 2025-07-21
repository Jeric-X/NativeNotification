using NativeNotification.Interface;
using NativeNotification.Linux;

#if WINDOWS
using NativeNotification.Windows;
#endif

namespace NativeNotification;

public class ManagerFactory
{
    public static INotificationManager GetNotificationManager(ManagerConfig? config = default)
    {
#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            return new WindowsNotificationManager();
        }
#endif
        if (OperatingSystem.IsLinux())
        {
            return new DBusNotificationManager(config);
        }

        throw new NotSupportedException("Notification manager not supported on this platform.");
    }
}
