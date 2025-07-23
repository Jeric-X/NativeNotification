using NativeNotification.Interface;

namespace NativeNotification.Common;

public abstract class NotificationManagerBase : INotificationManager, INotificationManagerInternal
{
    protected SessionHistory SessionHistory { get; } = new();

    private INotification? _shared;

    public event Action<NotificationActivatedEventArgs>? ActionActivated;

    public INotification Shared
    {
        get
        {
            _shared ??= Create();
            return _shared;
        }
    }

    public virtual bool IsAppLaunchedByNotification => false;
    public virtual string? LaunchActionId { get; } = null;

    public abstract INotification Create();
    public virtual IProgressNotification CreateProgress() => throw new NotSupportedException("Progress notifications are not supported on current platform.");
    public abstract void RomoveAllNotifications();

    protected virtual bool RemoveNotificationOnContentClick => true;

    public void RemoveHistory(string notificationId)
    {
        SessionHistory.Remove(notificationId);
    }

    public void AddHistory(string notificationId, INotificationInternal notification)
    {
        SessionHistory.AddSession(notificationId, notification);
    }

    public virtual void ActivateNotification(string notificationId, string? actionId, INotification? notification = null, bool isLaunchedByNotification = false)
    {
        notification ??= SessionHistory.GetNotification(notificationId);
        if (actionId is not null)
        {
            var action = SessionHistory.GetAction(notificationId, actionId);
            action?.Invoke();
            SessionHistory.Remove(notificationId);
        }
        else
        {
            notification?.ContentAction?.Invoke();
            if (OperatingSystem.IsMacOS() is false)
            {
                SessionHistory.Remove(notificationId);
            }
            else if (RemoveNotificationOnContentClick)
            {
                notification?.Remove();
            }
        }

        var EventArgs = new NotificationActivatedEventArgs(notificationId)
        {
            ActionId = actionId,
            Notification = notification,
            Type = actionId is not null ? ActivatedType.ActionButtonClicked : ActivatedType.ContentClicked,
            IsLaunchedActivation = isLaunchedByNotification
        };

        ActionActivated?.Invoke(EventArgs);
    }

    protected void TriggerActivationEvent(NotificationActivatedEventArgs eventArgs)
    {
        ActionActivated?.Invoke(eventArgs);
    }

    public virtual IEnumerable<INotification> GetAllNotifications()
    {
        return SessionHistory.GetAll();
    }
}
