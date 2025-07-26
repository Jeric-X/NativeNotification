namespace NativeNotification.Interface;

public interface INotificationManager
{
    bool IsAppLaunchedByNotification { get; }
    event Action<NotificationActivatedEventArgs>? ActionActivated;

    /// <summary>
    /// Returns a shared singleton <c>INotification</c> instance that can be used to create notifications.
    /// </summary>
    INotification Shared { get; }
    INotification Create();

    /// <summary>
    /// If the current platform supports progress notifications, returns an <c>IProgressNotification</c> instance; otherwise throws an exception;
    /// </summary>
    IProgressNotification CreateProgress();

    INotification Show(string title, string message, IEnumerable<ActionButton>? actions);
    INotification Show(string title, string message);

    /// <summary>
    /// If the current platform supports progress notifications, returns an <c>IProgressNotification</c> instance; otherwise,
    /// if suppressNotSupportedException is false, throws an exception;
    /// if suppressNotSupportedException is true, returns an dummy implementation of <c>IProgressNotification</c>.
    /// </summary>
    IProgressNotification CreateProgress(bool suppressNotSupportedException);
    void RomoveAllNotifications();
    IEnumerable<INotification> GetAllNotifications();
}
