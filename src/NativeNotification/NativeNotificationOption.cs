namespace NativeNotification;

public class NativeNotificationOption
{
    /// <summary>
    /// Windows: A shortcut named after <c>AppName</c> will be created in the Start Menu.
    /// Linux: The name of the application, may be shown in the notification center.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// used on Linux.
    /// AppIcon should be either an URI (file:// is the only URI schema supported right now) or a name in a freedesktop.org-compliant icon theme (not a GTK+ stock ID).
    /// </summary>
    public string? AppIcon { get; set; }
}
