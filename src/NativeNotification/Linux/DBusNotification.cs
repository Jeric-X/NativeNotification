using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.Linux;

internal class DBusNotification(NotificationManagerConfig _config, IDbusNotifications _dBusInstance, ActionManager<uint> _actionManager) : INotification, INotificationInternal
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Uri? Image { get; set; }
    public List<ActionButton> Buttons { get; set; } = [];
    public bool IsAlive { get; set; } = false;
    public void SetIsAlive(bool IsAlive) => this.IsAlive = IsAlive;

    private ExpirationHelper? _expirationHelper;
    private uint? _notificationId;
    private uint NotificationId => _notificationId ?? 0;
    private DateTimeOffset? _expirationTime;

    protected void NativeShow(NotificationConfig? config = null)
    {
        List<string> actionList = [];
        foreach (var button in Buttons)
        {
            actionList.Add(button.Uid.ToString());
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

        var duration = ExpirationHelper.GetExpirationDuration(config);
        if (duration.HasValue)
        {
            _expirationTime = DateTimeOffset.Now.Add(duration.Value);
        }
        else if (_expirationTime.HasValue)
        {
            duration = _expirationTime.Value - DateTimeOffset.Now;
        }

        _notificationId = Task.Run(async () =>
            await _dBusInstance.NotifyAsync(
                _config.AppName,
                NotificationId,
                _config.AppIcon,
                Title ?? string.Empty,
                Message ?? string.Empty,
                [.. actionList],
                hintDictionary,
                (int?)duration?.TotalMilliseconds ?? 0
            )
        ).Result;
        IsAlive = true;

        _actionManager.AddSession(NotificationId, this);
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
        _dBusInstance.CloseNotificationAsync(NotificationId);
    }
}