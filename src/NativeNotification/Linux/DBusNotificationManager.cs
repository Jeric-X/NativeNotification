using NativeNotification.Common;
using NativeNotification.Interface;
using System.Runtime.Versioning;
using Tmds.DBus;

namespace NativeNotification.Linux;

[SupportedOSPlatform("linux")]
internal sealed class DBusNotificationManager : NotificationManagerBase<uint>, IDisposable
{
    private bool _disposed = false;
    public IDbusNotifications DBus { get; }

    private readonly List<IDisposable> _disposables = [];
    private readonly NativeNotificationOption _config;

    public DBusNotificationManager(NativeNotificationOption? config = default)
    {
        _config = config ?? new NativeNotificationOption();

        DBus = Connection.Session.CreateProxy<IDbusNotifications>("org.freedesktop.Notifications", "/org/freedesktop/Notifications");
        var watchInvokeTask = DBus.WatchActionInvokedAsync(input => ActivateAction(input.id, input.actionKey));
        var watchClosedTask = DBus.WatchNotificationClosedAsync(input => RemoveHistory(input.id));
        _disposables.Add(watchInvokeTask.Result);
        _disposables.Add(watchClosedTask.Result);
    }

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        RomoveAllNotifications();
        _disposables.ForEach(disposable => disposable.Dispose());
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    public override INotification Create()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(DBusNotificationManager));
        return new DBusNotification(_config, this);
    }

    public override IProgressNotification CreateProgress()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(DBusNotificationManager));
        throw new NotImplementedException();
    }

    public override void RomoveAllNotifications()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(DBusNotificationManager));
        SessionHistory.RemoveAll();
    }
    ~DBusNotificationManager() => Dispose();
}