namespace NativeNotification.Interface;

public interface IProgressNotification : INotification
{
    public string? ProgressTitle { get; set; }
    public double? ProgressValue { get; set; }
    public string? ProgressValueTip { get; set; }
    public bool IsIndeterminate { get; set; }
}
