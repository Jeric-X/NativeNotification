using NativeNotification.Interface;

namespace NativeNotification.Common;

public interface INotificationInternal : INotification
{
    void SetIsAlive(bool IsAlive);
}
