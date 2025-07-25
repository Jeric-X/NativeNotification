﻿using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.MacOS;

internal class NSNotificationManager : NotificationManagerBase, INotificationManager
{
    public NSUserNotificationCenter Center { get; }
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
            foreach (var deliveredNativeNotification in Center.DeliveredNotifications)
            {
                if (deliveredNativeNotification.Identifier is not null)
                {
                    var notification = new NSNotification(this, deliveredNativeNotification);
                    SessionHistory.AddSession(notification.NotificationId, notification);
                }
            }

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
        SessionHistory.Reset();
    }

    public override IProgressNotification CreateProgress(bool suppressNotSupportedException)
    {
        if (suppressNotSupportedException)
        {
            return new DummyProgressNotification();
        }
        throw new NotSupportedException("ProgressNotification is not support on this platform.");
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

        var notification = SessionHistory.GetNotification(nsUserNotification.Identifier);
        if (nsUserNotification.ActivationType == NSUserNotificationActivationType.ContentsClicked)
        {
            ActivateContentClicked(nsUserNotification.Identifier, notification, isLaunchApp);
        }
        else if (nsUserNotification.ActivationType == NSUserNotificationActivationType.ActionButtonClicked)
        {
            var actionId = nsUserNotification.UserInfo?[NSNotification.ActionButtonIdKey] as NSString;
            ActivateButtonClicked(nsUserNotification.Identifier, actionId, notification, isLaunchApp);
        }
        else if (nsUserNotification.ActivationType != NSUserNotificationActivationType.AdditionalActionClicked)
        {
            return;
        }

        if (nsUserNotification.Identifier is not null && nsUserNotification.AdditionalActivationAction?.Identifier is not null)
        {
            ActivateButtonClicked(nsUserNotification.Identifier, nsUserNotification.AdditionalActivationAction.Identifier, notification, isLaunchApp);
        }
        else if (nsUserNotification.Identifier is not null)
        {
            RemoveHistory(nsUserNotification.Identifier);
        }
    }
}