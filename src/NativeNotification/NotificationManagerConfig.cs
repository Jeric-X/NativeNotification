namespace NativeNotification;

public class NotificationManagerConfig
{
    /// <summary>
    /// Only used on Linux.
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// Only used on Linux.
    /// AppIcon should be either an URI (file:// is the only URI schema supported right now) or a name in a freedesktop.org-compliant icon theme (not a GTK+ stock ID).
    /// </summary>
    public string AppIcon { get; set; } = string.Empty;
}
