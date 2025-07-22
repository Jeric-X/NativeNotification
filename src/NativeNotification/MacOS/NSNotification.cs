#if MACOS

using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.MacOS;

internal class NSNotification(NSUserNotificationCenter _center, ActionManager<string> _actionManager) : INotification, INotificationInternal
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Uri? Image { get; set; }
    public List<ActionButton> Buttons { get; set; } = [];
    public bool IsAlive { get; set; } = false;
    public void SetIsAlive(bool IsAlive) => this.IsAlive = IsAlive;

    private ExpirationHelper? _expirationHelper;
    private readonly string _notificationId = GetGuid();
    private DateTimeOffset? _expirationTime;

    private static string GetGuid()
    {
        return Guid.NewGuid().ToString();
    }

    protected void NativeShow(NotificationConfig? config = null)
    {
        config ??= new NotificationConfig();
        config.ExpirationTime ??= _expirationTime;
        List<NSUserNotificationAction> actionList = [];
        foreach (var button in Buttons)
        {
            actionList.Add(NSUserNotificationAction.GetAction(button.Uid, button.Text));
        }

        var notification = new NSUserNotification
        {
            Identifier = _notificationId,
            Title = Title,
            InformativeText = Message,
            SoundName = config?.Silent is true ? null : NSUserNotification.NSUserNotificationDefaultSoundName,
            AdditionalActions = [.. actionList],
            HasActionButton = true,
            ActionButtonTitle = "Action"
        };
    
        if (Image is not null)
        {
            notification.ContentImage = new NSImage(Image!);
        }

        _center.DeliverNotification(notification);
        _actionManager.AddSession(_notificationId, this);
        var duration = ExpirationHelper.GetExpirationDuration(config);
        if (duration.HasValue)
        {
            _expirationTime = DateTimeOffset.Now.Add(duration.Value);
        }
        _expirationHelper ??= new ExpirationHelper(this);
        _expirationHelper.SetNoficifationDuration(config);
    }

    public void Show(NotificationConfig? config = null)
    {
        NativeShow(config);
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
        _center.RemoveDeliveredNotification(new NSUserNotification { Identifier = _notificationId });
        _actionManager.OnClosed(_notificationId);
    }
}

#endif