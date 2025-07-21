using NativeNotification.Interface;
#if WINDOWS
using NativeNotification.Windows;
#endif

namespace NativeNotification;

public class ManagerFactory
{
    public static INotificationManager GetNotificationManager()
    {
#if WINDOWS
        return new WindowsNotificationManager();
#endif
        throw new NotSupportedException("Notification manager not supported on this platform.");
    }
}
