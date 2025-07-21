namespace NativeNotification.Interface;

public class NotificationConfig
{
    /// <summary>
    /// If both ExpirationTime and Duration are configured, the option that triggers earlier will take effect.
    /// </summary>
    public DateTime? ExpirationTime { get; set; } = null;

    /// <summary>
    /// If both ExpirationTime and Duration are configured, the option that triggers earlier will take effect.
    /// </summary>
    public TimeSpan? Duration { get; set; } = null;

    public bool Silent { get; set; } = false;
}