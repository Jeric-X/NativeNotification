namespace NativeNotification.Interface;

public interface INotification
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Uri? Image { get; set; }
    public List<ActionButton> Buttons { get; set; }

    public void Show(NotificationConfig? config = null);

    /// <summary>
    /// Image and Buttons are not updated on Windows.
    /// </summary>
    public bool Update();
    public void Remove();
}