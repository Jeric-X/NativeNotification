using NativeNotification.Common;
using NativeNotification.Interface;
using System.Runtime.Versioning;
using Tmds.DBus;

namespace NativeNotification.Linux;

[SupportedOSPlatform("linux")]
internal sealed class DBusNotificationManager : INotificationManager, IDisposable
{
    private readonly IDbusNotifications _dBusInstance;
    private readonly ActionManager<uint> _actionManager = new();
    private readonly List<IDisposable> _disposables = [];
    private readonly NotificationManagerConfig _config;

    public DBusNotificationManager(NotificationManagerConfig? config = default)
    {
        var connection = Connection.Session;
        _dBusInstance = connection.CreateProxy<IDbusNotifications>("org.freedesktop.Notifications", "/org/freedesktop/Notifications");
        Task.Run(async () =>
        {
            _disposables.Add(await _dBusInstance.WatchActionInvokedAsync(input => _actionManager.OnActivated(input.id, input.actionKey)));
            _disposables.Add(await _dBusInstance.WatchNotificationClosedAsync(input => _actionManager.OnClosed(input.id)));
        }).Wait();
        _config = config ?? new NotificationManagerConfig();
    }

    public void Dispose()
    {
        _disposables.ForEach(disposable => disposable.Dispose());
        _actionManager.RemoveAll();
        GC.SuppressFinalize(this);
    }

    public INotification Create()
    {
        return new DBusNotification(_config, _dBusInstance, _actionManager);
    }

    public IProgressNotification CreateProgress()
    {
        throw new NotImplementedException();
    }

    public void RomoveAllNotifications()
    {
        _actionManager.RemoveAll();
    }

    ~DBusNotificationManager() => Dispose();
}