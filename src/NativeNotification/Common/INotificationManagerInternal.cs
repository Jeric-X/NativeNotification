namespace NativeNotification.Common;

internal interface INotificationManagerInternal<T> where T : notnull
{
    void RemoveHistory(T notificationId);
}
