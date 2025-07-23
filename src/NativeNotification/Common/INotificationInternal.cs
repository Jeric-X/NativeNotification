using NativeNotification.Interface;

namespace NativeNotification.Common;

public interface INotificationInternal<IdType> : INotification
{
    void SetIsAlive(bool IsAlive);
    IdType NotificationId { get; }
}
