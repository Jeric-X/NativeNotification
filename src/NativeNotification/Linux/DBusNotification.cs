using NativeNotification.Common;
using NativeNotification.Interface;
using System.Runtime.Versioning;

namespace NativeNotification.Linux;

[SupportedOSPlatform("linux")]
internal class DBusNotification(NativeNotificationOption _config, DBusNotificationManager _manager) : INotification, INotificationInternal
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Uri? Image { get; set; }
    public List<ActionButton> Buttons { get; set; } = [];
    public bool IsAlive { get; set; } = false;
    public void SetIsAlive(bool IsAlive) => this.IsAlive = IsAlive;

    public bool IsCreatedByCurrentProcess { get; } = true;
    private ExpirationHelper? _expirationHelper;
    private uint? _notificationId;
    public uint InternalNotificationId => _notificationId ?? 0;
    public string NotificationId => InternalNotificationId.ToString();

    public Action? ContentAction { get; set; }

    private DateTimeOffset? _expirationTime;

    protected void NativeShow(NotificationDeliverOption? config = null)
    {
        List<string> actionList = [];
        foreach (var button in Buttons)
        {
            actionList.Add(button.ActionId.ToString());
            actionList.Add(button.Text);
        }

        var hintDictionary = new Dictionary<string, object>();
        if (Image is not null)
        {
            hintDictionary.Add("image-path", Image.AbsoluteUri);
        }

        if (config?.Silent is true)
        {
            hintDictionary.Add("suppress-sound", true);
        }

        if (config is null && _expirationTime is not null)
        {
            config = new NotificationDeliverOption
            {
                ExpirationTime = _expirationTime
            };
        }
        var duration = ExpirationHelper.GetExpirationDuration(config);

        _manager.AddHistory(NotificationId, this);
        _notificationId = Task.Run(async () =>
            await _manager.DBus.NotifyAsync(
                _config.AppName ?? string.Empty,
                InternalNotificationId,
                _config.AppIcon ?? string.Empty,
                Title ?? string.Empty,
                Message ?? string.Empty,
                [.. actionList],
                hintDictionary,
                (int?)duration?.TotalMilliseconds ?? 0
            )
        ).Result;
        IsAlive = true;

        if (duration.HasValue)
        {
            _expirationTime = DateTimeOffset.Now.Add(duration.Value);
        }
    }

    public void Show(NotificationDeliverOption? config = null)
    {
        NativeShow(config ?? new());
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
        _manager.DBus.CloseNotificationAsync(InternalNotificationId);
    }
}