#if MACOS

using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.MacOS;

internal class NSNotificationManager : NotificationManagerBase, INotificationManager
{
    public NSUserNotificationCenter Center { get; }
    private readonly SessionHistory _actionManager = new();
    private bool _isAppLaunchedByNotification = false;
    public override bool IsAppLaunchedByNotification => _isAppLaunchedByNotification;
    private NSObject? _didFinishLaunchingObserver = null;
    private readonly NativeNotificationOption _config;
    protected override bool RemoveNotificationOnContentClick => _config.RemoveNotificationOnContentClick;

    public NSNotificationManager(NativeNotificationOption? config = default)
    {
        _config = config ?? new NativeNotificationOption();
        Center = NSUserNotificationCenter.DefaultUserNotificationCenter;
        ArgumentNullException.ThrowIfNull(Center, nameof(Center));
        _didFinishLaunchingObserver = NSApplication.Notifications.ObserveDidFinishLaunching((sender, arg) =>
        {
            var nsUserNotification = arg.Notification?.UserInfo?[NSApplication.LaunchUserNotificationKey] as NSUserNotification;
            _isAppLaunchedByNotification = arg.IsLaunchFromUserNotification && nsUserNotification is not null;
            if (nsUserNotification is not null)
            {
                HandleActivateNotification(nsUserNotification, true);
            }
            _didFinishLaunchingObserver?.Dispose();
            _didFinishLaunchingObserver = null;
        });

        Center.DidActivateNotification += (sender, e) => HandleActivateNotification(e.Notification);
        Center.DidDeliverNotification += (sender, e) =>
        {
            if (e.Notification.Identifier is string id)
            {
                SessionHistory.GetNotification(id)?.SetIsAlive(true);
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

    public override IEnumerable<INotificationInternal> GetAllNotifications()
    {
        var existIds = Center.DeliveredNotifications.Select(x => x.Identifier);
        return SessionHistory.GetAll().Where(x => existIds.Contains(x.NotificationId));
    }

    private void HandleActivateNotification(NSUserNotification nsUserNotification, bool isLaunchApp = false)
    {
        if (nsUserNotification.Identifier is null)
        {
            return;
        }

        if (nsUserNotification.ActivationType == NSUserNotificationActivationType.ContentsClicked)
        {
            ActivateNotification(nsUserNotification.Identifier, null, null, isLaunchApp);
        }
        if (nsUserNotification.ActivationType != NSUserNotificationActivationType.AdditionalActionClicked)
        {
            return;
        }

        if (nsUserNotification.Identifier is not null && nsUserNotification.AdditionalActivationAction?.Identifier is not null)
        {
            ActivateNotification(nsUserNotification.Identifier, nsUserNotification.AdditionalActivationAction.Identifier, null, isLaunchApp);
        }
        else if (nsUserNotification.Identifier is not null)
        {
            RemoveHistory(nsUserNotification.Identifier);
        }
    }
}

#endif