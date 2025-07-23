using NativeNotification.Interface;

namespace NativeNotification.Common;

public abstract class NotificationManagerBase<TNotificationId> 
    : INotificationManager, INotificationManagerInternal<TNotificationId> where TNotificationId : notnull
{
    protected SessionHistory<TNotificationId> SessionHistory { get; } = new(); 

    private INotification? _shared;

    public event Action<string>? ActionActived;

    public INotification Shared
    {
        get
        {
            _shared ??= Create();
            return _shared;
        }
    }

    public abstract INotification Create();
    public abstract IProgressNotification CreateProgress();
    public abstract void Dispose();
    public abstract void RomoveAllNotifications();

    public void RemoveHistory(TNotificationId notificationId)
    {
        SessionHistory.Remove(notificationId);
    }

    public void AddHistory(TNotificationId notificationId, INotificationInternal<TNotificationId> notification)
    {
        SessionHistory.AddSession(notificationId, notification);
    }

    public void ActivateAction(TNotificationId notificationId, string actionId)
    {
        var action = SessionHistory.GetAction(notificationId, actionId);
        action?.Invoke();
        ActionActived?.Invoke(actionId);
        SessionHistory.Remove(notificationId);
    }

    public virtual IEnumerable<INotification> GetAllNotifications()
    {
        return SessionHistory.GetAll();
    }
}
