#if MACOS

using NativeNotification.Common;
using NativeNotification.Interface;
using NativeNotification.Linux;

namespace NativeNotification.MacOS;

internal class NSNotificationManager : NotificationManagerBase<string>, INotificationManager
{
    private bool _disposed = false;
    public NSUserNotificationCenter Center { get; }
    private readonly SessionHistory<string> _actionManager = new();

    public NSNotificationManager()
    {
        Center = NSUserNotificationCenter.DefaultUserNotificationCenter;
        ArgumentNullException.ThrowIfNull(Center, nameof(Center));

        Center.DidActivateNotification += (sender, e) =>
        {
            if (e.Notification.Identifier is not null && e.Notification.AdditionalActivationAction?.Identifier is not null)
            {
                ActivateAction(e.Notification.Identifier, e.Notification.AdditionalActivationAction.Identifier);
            }
            else if (e.Notification.Identifier is not null)
            {
                RemoveHistory(e.Notification.Identifier);
            }

        };
        Center.DidDeliverNotification += (sender, e) =>
        {
            if (e.Notification.Identifier is string id)
            {
                SessionHistory.Get(id)?.SetIsAlive(true);
            }
        };
        Center.ShouldPresentNotification = (center, notification) =>
        {
            return true;
        };
    }

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        RomoveAllNotifications();
        Center.Dispose();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    public override INotification Create()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(NSNotificationManager));
        return new NSNotification(this);
    }

    public override IProgressNotification CreateProgress()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(NSNotificationManager));
        throw new NotImplementedException();
    }

    public override void RomoveAllNotifications()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(NSNotificationManager));
        Center.RemoveAllDeliveredNotifications();
        _actionManager.Reset();
    }

    public override IEnumerable<INotificationInternal<string>> GetAllNotifications()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(DBusNotificationManager));
        var existIds = Center.DeliveredNotifications.Select(x => x.Identifier);
        return SessionHistory.GetAll().Where(x => existIds.Contains(x.NotificationId));
    }

    ~NSNotificationManager() => Dispose();
}

#endif