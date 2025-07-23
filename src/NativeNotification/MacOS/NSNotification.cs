#if MACOS

using CoreBluetooth;
using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.MacOS;

internal class NSNotification(NSNotificationManager _manager) : INotification, INotificationInternal<string>
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Uri? Image { get; set; }
    public List<ActionButton> Buttons { get; set; } = [];

    private bool isAlive = false;
    public bool IsAlive
    {
        get
        {
            if (isAlive is false)
            {
                return false;
            }
            isAlive = _manager.GetAllNotifications().Any(x => x.NotificationId == NotificationId);
            return isAlive;
        }
        set => isAlive = value;
    }
    public void SetIsAlive(bool IsAlive) => this.IsAlive = IsAlive;

    private ExpirationHelper? _expirationHelper;

    public string NotificationId { get; } = GetGuid();

    private static string GetGuid()
    {
        return Guid.NewGuid().ToString();
    }

    protected void NativeShow(NotificationDeliverOption? config = null)
    {
        List<NSUserNotificationAction> actionList = [];
        foreach (var button in Buttons)
        {
            actionList.Add(NSUserNotificationAction.GetAction(button.ActionId, button.Text));
        }

        var notification = new NSUserNotification
        {
            Identifier = NotificationId,
            Title = Title,
            InformativeText = Message,
            SoundName = config?.Silent is true ? null : NSUserNotification.NSUserNotificationDefaultSoundName,
            AdditionalActions = [.. actionList],
            HasActionButton = false,
        };

        if (Image is not null)
        {
            notification.ContentImage = new NSImage(Image!);
        }

        _manager.AddHistory(NotificationId, this);
        _manager.Center.DeliverNotification(notification);
    }

    public void Show(NotificationDeliverOption? config = null)
    {
        NativeShow(config);
        _expirationHelper ??= new ExpirationHelper(this);
        _expirationHelper.SetNoficifationDuration(config);
    }

    public bool Update()
    {
        if (!IsAlive)
        {
            return false;
        }
        NativeShow();
        return true;
    }

    public void Remove()
    {
        _manager.Center.RemoveDeliveredNotification(new NSUserNotification { Identifier = NotificationId });
        _manager.RemoveHistory(NotificationId);
    }
}

#endif