namespace NativeNotification.Interface;

public interface INotificationManager : IDisposable
{
    INotification Create();
    IProgressNotification CreateProgress();
    void RomoveAllNotifications();
}
