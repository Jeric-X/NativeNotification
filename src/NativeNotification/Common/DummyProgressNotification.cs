using NativeNotification.Interface;

namespace NativeNotification.Common;

internal class DummyProgressNotification : IProgressNotification
{
    public string? ProgressTitle { get; set; }
    public double? ProgressValue { get; set; }
    public string? ProgressValueTip { get; set; }
    public bool IsIndeterminate { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Uri? Image { get; set; }
    public List<ActionButton> Buttons { get; set; } = [];
    public Action? ContentAction { get; set; }
    public string NotificationId { get; } = Guid.NewGuid().ToString();
    public bool IsAlive { get; } = false;
    public bool IsCreatedByCurrentProcess { get; } = true;

    public void Remove()
    {
    }

    public void Show(NotificationDeliverOption? config = null)
    {
    }

    public bool Update()
    {
        return false;
    }
}
