namespace NativeNotification.Interface;

public interface INotificationManager
{
    bool IsApplicationStartedByNotificationAction { get; }
    string? LaunchActionId { get; }
    event Action<string>? ActionActived;
    INotification Shared { get; }
    INotification Create();
    IProgressNotification CreateProgress();
    void RomoveAllNotifications();
    IEnumerable<INotification> GetAllNotifications();
}
