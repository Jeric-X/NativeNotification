namespace NativeNotification.Interface;

public class NotificationActivatedEventArgs(string notificationId)
{
    public string NotificationId { get; init; } = notificationId;
    public string? ActionId { get; init; }
    public INotification? Notification { get; init; }
    public ActivatedType Type { get; init; } = ActivatedType.ActionButtonClicked;

    /// <summary>
    /// <c>true</c> if the program is launched by clicking the notification.
    /// </summary>
    public bool IsLaunchedActivation { get; init; } = false;
}
