#if MACOS

using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.MacOS;

internal class NSNotificationManager : INotificationManager
{
    private bool _disposed = false;
    private readonly NSUserNotificationCenter _nsNotificationCenter;
    private readonly ActionManager<string> _actionManager = new();

    public NSNotificationManager()
    {
        _nsNotificationCenter = NSUserNotificationCenter.DefaultUserNotificationCenter;
        // _nsNotificationCenter.Delegate = new NSUserNotificationCenterDelegateImpl();
        ArgumentNullException.ThrowIfNull(_nsNotificationCenter, nameof(_nsNotificationCenter));
        _nsNotificationCenter.DidActivateNotification += (sender, e) =>
        {
            Console.WriteLine($"Notification activated: {e.Notification.Identifier}");
            // _actionManager.OnActivated(new Guid(), e.Notification);
        };
        _nsNotificationCenter.DidDeliverNotification += (sender, e) =>
        {
            Console.WriteLine($"Notification Delivered: {e.Notification.Identifier}");
            // _actionManager.OnActivated(new Guid(), e.Notification);
        };
        _nsNotificationCenter.ShouldPresentNotification = (center, notification) =>
        {
            return true;
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        RomoveAllNotifications();
        _nsNotificationCenter.Dispose();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    public INotification Create()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(NSNotificationManager));
        return new NSNotification(_nsNotificationCenter, _actionManager);
    }

    public IProgressNotification CreateProgress()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(NSNotificationManager));
        throw new NotImplementedException();
    }

    public void RomoveAllNotifications()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(NSNotificationManager));
        _nsNotificationCenter.RemoveAllDeliveredNotifications();
        _actionManager.RemoveAll();
    }

    ~NSNotificationManager() => Dispose();
}

#endif