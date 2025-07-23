namespace NativeNotification.Interface;

public interface INotification
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Uri? Image { get; set; }

    /// <summary>
    /// You can set up to 5 <c>ActionButton</c>s on Windows, 3 on Linux, and very many on macOS.
    /// </summary>
    public List<ActionButton> Buttons { get; set; }
    public Action? ContentAction { get; set; }
    public string NotificationId { get; }

    public bool IsAlive { get; }

    /// <summary>
    /// <c>IsCreatedByCurrentProcess</c> indicates whether the notification was created by the current process.
    /// If value is <c>false</c>, it is recommended to reconfigure this <c>INotification</c> before calling <c>Update</c> or <c>Show</c>.
    /// </summary>
    public bool IsCreatedByCurrentProcess { get; }
    public void Show(NotificationDeliverOption? config = null);

    /// <summary>
    /// Image and Buttons are not updated on Windows.
    /// </summary>
    public bool Update();
    public void Remove();
}