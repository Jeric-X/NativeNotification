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
    public static INotificationManager GetNotificationManager(NativeNotificationOption? option = default)
    {
#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            return new WindowsNotificationManager(option);
        }
#endif
#if MACOS
        if (OperatingSystem.IsMacOS())
        {
            return new NSNotificationManager(option);
        }
#endif
        if (OperatingSystem.IsLinux())
        {
            return new DBusNotificationManager(option);
        }

        throw new NotSupportedException("Notification manager not supported on this platform.");
    }
}
