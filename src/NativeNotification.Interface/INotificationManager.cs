namespace NativeNotification.Interface;

public interface INotificationManager
{
    bool IsAppLaunchedByNotification { get; }
    event Action<NotificationActivatedEventArgs>? ActionActivated;
    INotification Shared { get; }
    INotification Create();
    IProgressNotification CreateProgress();
    void RomoveAllNotifications();
    IEnumerable<INotification> GetAllNotifications();
}
