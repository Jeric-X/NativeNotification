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
            if (value is not null)
            {
                _nsImage = new NSImage(value!);
            }
            else
            {
                _nsImage = null;
            }
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

    internal static readonly NSString ActionButtonIdKey = new("_NativeNotification._ActionButtonId");
    private ExpirationHelper? _expirationHelper;
    private readonly NSNotificationManager _manager;

    public NSNotification(NSNotificationManager manager)
    {
        _manager = manager;
    }

    public NSNotification(NSNotificationManager manager, NSUserNotification notification)
    {
        NotificationId = notification.Identifier ?? GetGuid();
        _manager = manager;
        Title = notification.Title;
        Message = notification.InformativeText;
        _nsImage = notification.ContentImage;
        var actionButtonId = notification.UserInfo?[ActionButtonIdKey] as NSString;
        if (actionButtonId is not null)
        {
            var action = new ActionButton(notification.ActionButtonTitle, actionButtonId.ToString());
            var additionalActions = notification.AdditionalActions?
                .Where(x => x.Identifier is not null && x.Title is not null)
                .Select(x => new ActionButton(x.Title!, x.Identifier!)) ?? [];
            Buttons.Add(action);
            Buttons.AddRange(additionalActions);
        }

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

        var additionalActions = Buttons.Skip(1)
            .Select(button => NSUserNotificationAction.GetAction(button.ActionId, button.Text))
            .ToArray();

        var notification = new NSUserNotification
        {
            Identifier = NotificationId,
            Title = Title,
            InformativeText = Message,
            SoundName = config?.Silent is true ? null : NSUserNotification.NSUserNotificationDefaultSoundName,
            AdditionalActions = additionalActions,
            HasActionButton = Buttons.Count > 0,
            ActionButtonTitle = Buttons.Count > 0 ? Buttons[0].Text : string.Empty,
            ContentImage = _nsImage
        };
        if (Buttons.Count > 0)
        {
            notification.UserInfo = new NSMutableDictionary()
            {
                [ActionButtonIdKey] = new NSString(Buttons[0].ActionId)
            };
        }
        notification.SetValueForKey(NSNumber.FromBoolean(true), new NSString("_showsButtons"));

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