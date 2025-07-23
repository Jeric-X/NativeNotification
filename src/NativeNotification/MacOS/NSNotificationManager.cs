#if MACOS

using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.MacOS;

internal class NSNotificationManager : NotificationManagerBase<string>, INotificationManager
{
    public NSUserNotificationCenter Center { get; }
    private readonly SessionHistory<string> _actionManager = new();
    private bool _isApplicationStartedByNotificationAction = false;
    public override bool IsApplicationStartedByNotificationAction => _isApplicationStartedByNotificationAction;
    public string? _launchActionId = null;
    public override string? LaunchActionId => _launchActionId;

    public NSNotificationManager()
    {
        Center = NSUserNotificationCenter.DefaultUserNotificationCenter;
        ArgumentNullException.ThrowIfNull(Center, nameof(Center));
        var token = NSApplication.Notifications.ObserveDidFinishLaunching((sender, arg) =>
        {
            _isApplicationStartedByNotificationAction = arg.IsLaunchFromUserNotification;
            var nsUserNotification = arg.Notification?.UserInfo?[NSApplication.LaunchUserNotificationKey] as NSUserNotification;
            if (nsUserNotification is not null)
            {
                _launchActionId = nsUserNotification.AdditionalActivationAction?.Identifier;
                if (_launchActionId is not null)
                {
                    ActivateAction(_launchActionId);
                }
            }
        });

        Center.DidActivateNotification += (sender, e) =>
        {
            if (e.Notification.ActivationType != NSUserNotificationActivationType.AdditionalActionClicked)
            {
                return;
            }

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

    public override INotification Create()
    {
        return new NSNotification(this);
    }

    public override void RomoveAllNotifications()
    {
        Center.RemoveAllDeliveredNotifications();
        _actionManager.Reset();
    }

    public override IEnumerable<INotificationInternal<string>> GetAllNotifications()
    {
        var existIds = Center.DeliveredNotifications.Select(x => x.Identifier);
        return SessionHistory.GetAll().Where(x => existIds.Contains(x.NotificationId));
    }
}

#endif