using System.Runtime.Versioning;

namespace NativeNotification.Interface;

public enum ActivatedType
{
    ActionButtonClicked,

    /// <summary>
    /// Not supported on Linux.
    /// </summary>
    ContentClicked,
}