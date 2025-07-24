using NativeNotification.Interface;
#if MACOS
using NativeNotification.MacOS;
#elif WINDOWS
using NativeNotification.Windows;
#else
using NativeNotification.Linux;
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
#elif MACOS
        if (OperatingSystem.IsMacOS())
        {
            return new NSNotificationManager(option);
        }
#else
        if (OperatingSystem.IsLinux())
        {
            return new DBusNotificationManager(option);
        }
#endif
        throw new NotSupportedException("Notification manager not supported on this platform.");
    }
}
