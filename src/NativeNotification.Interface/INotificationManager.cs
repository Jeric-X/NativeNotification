namespace NativeNotification.Interface;

public interface INotificationManager : IDisposable
{
    event Action<string>? ActionActived;
    INotification Shared { get; }
    INotification Create();
    IProgressNotification CreateProgress();
    void RomoveAllNotifications();
    IEnumerable<INotification> GetAllNotifications();
}
