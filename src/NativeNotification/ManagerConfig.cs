using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativeNotification;

public class ManagerConfig
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
