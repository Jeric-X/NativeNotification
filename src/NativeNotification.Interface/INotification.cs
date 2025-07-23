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
    public void Show(NotificationDeliverOption? config = null);

    /// <summary>
    /// Image and Buttons are not updated on Windows.
    /// </summary>
    public bool Update();
    public void Remove();
}