using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.MacOS;

internal class NSNotification : INotification, INotificationInternal
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    private NSImage? _nsImage;
    private Uri? _imageUri;
    public Uri? Image
    {
        get
        {
            return _imageUri;
        }
        set
        {
            _imageUri = value;
            _nsImage = new NSImage(value!);
        }
    }
    public List<ActionButton> Buttons { get; set; } = [];
    public Action? ContentAction { get; set; }

    private bool _isAlive = false;
    public bool IsAlive
    {
        get
        {
            if (_isAlive is false)
            {
                return false;
            }
            _isAlive = _manager.GetAllNotifications().Any(x => x.NotificationId == NotificationId);
            return _isAlive;
        }
        set => _isAlive = value;
    }
    public void SetIsAlive(bool IsAlive) => this.IsAlive = IsAlive;

    private ExpirationHelper? _expirationHelper;
    private readonly NSNotificationManager _manager;

    public NSNotification(NSNotificationManager manager)
    {
        _manager = manager;
    }

    public NSNotification(NSNotificationManager manager, NSUserNotification notification)
    {
        _manager = manager;
        Title = notification.Title;
        Message = notification.InformativeText;
        _nsImage = notification.ContentImage;
        Buttons = notification.AdditionalActions?
            .Where(x => x.Identifier is not null && x.Title is not null)
            .Select(x => new ActionButton(x.Title!, x.Identifier!)).ToList() ?? [];
        ContentAction = null;
        IsAlive = true;
        IsCreatedByCurrentProcess = false;
    }

    public string NotificationId { get; } = GetGuid();

    public bool IsCreatedByCurrentProcess { get; private init; } = true;

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
            ContentImage = _nsImage
        };

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