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
    public abstract IProgressNotification CreateProgress(bool suppressNotSupportedException);
    public IProgressNotification CreateProgress() => CreateProgress(false);
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

    internal virtual void ActivateContentClicked(string notificationId, INotification? notification = null, bool isLaunchedByNotification = false)
    {
        notification ??= SessionHistory.GetNotification(notificationId);
        if (OperatingSystem.IsMacOS() is false)
        {
            SessionHistory.Remove(notificationId);
        }
        else if (RemoveNotificationOnContentClick)
        {
            notification?.Remove();
        }

        var EventArgs = new NotificationActivatedEventArgs(notificationId)
        {
            Notification = notification ?? SessionHistory.GetNotification(notificationId),
            Type = ActivatedType.ContentClicked,
            IsLaunchedActivation = isLaunchedByNotification
        };

        notification?.ContentAction?.Invoke();
        ActionActivated?.Invoke(EventArgs);
    }

    internal virtual void ActivateButtonClicked(string notificationId, string? actionId, INotification? notification = null, bool isLaunchedByNotification = false)
    {
        if (actionId is not null)
        {
            var action = SessionHistory.GetAction(notificationId, actionId);
            action?.Invoke();
            SessionHistory.Remove(notificationId);
        }
        var EventArgs = new NotificationActivatedEventArgs(notificationId)
        {
            ActionId = actionId,
            Notification = notification ?? SessionHistory.GetNotification(notificationId),
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

    public INotification Show(string title, string message, IEnumerable<ActionButton>? actions)
    {
        var notification = Create();
        notification.Title = title;
        notification.Message = message;
        if (actions is not null)
        {
            notification.Buttons.AddRange(actions);
        }
        notification.Show();
        return notification;
    }

    public INotification Show(string title, string message)
    {
        return Show(title, message, null);
    }
}
