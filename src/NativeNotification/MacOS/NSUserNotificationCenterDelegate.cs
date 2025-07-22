using AppKit;
using Foundation;

namespace NativeNotification.MacOS;

public class NSUserNotificationCenterDelegateImpl : NSUserNotificationCenterDelegate
{
    public override void DidDeliverNotification(NSUserNotificationCenter center, NSUserNotification notification)
    {
        Console.WriteLine($"Notification Delivered: {notification.Identifier}");
    }

    public override void DidActivateNotification(NSUserNotificationCenter center, NSUserNotification notification)
    {
        Console.WriteLine($"Notification Activated: {notification.Identifier}");
    }

    public override bool ShouldPresentNotification(NSUserNotificationCenter center, NSUserNotification notification)
    {
        // Always present the notification, even if the app is frontmost
        return true;
    }
}