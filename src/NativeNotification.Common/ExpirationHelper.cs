using NativeNotification.Interface;

namespace NativeNotification.Common;

public class ExpirationHelper(INotification session)
{
    public static TimeSpan? GetExpirationDuration(NotificationConfig? config)
    {
        TimeSpan? timeSpan = null;
        if (config?.ExpirationTime is not null)
        {
            if (config.ExpirationTime.Value < DateTimeOffset.Now)
            {
                timeSpan = TimeSpan.Zero;
            }
            else
            {
                timeSpan = config.ExpirationTime.Value - DateTimeOffset.Now;
            }
        }
        if (config?.Duration is not null)
        {
            if (timeSpan is null)
            {
                return config.Duration.Value;
            }
            return timeSpan < config.Duration.Value
                ? timeSpan.Value
                : config.Duration.Value;
        }
        return null;
    }

    private CancellationTokenSource? _durationCts;
    private CancellationToken CreateNewDurationCtk()
    {
        var cts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _durationCts, null);
        oldCts?.Cancel();
        oldCts?.Dispose();
        return cts.Token;
    }

    public async void SetNoficifationDuration(NotificationConfig? config = null)
    {
        var duration = GetExpirationDuration(config);
        if (duration is not null)
        {
            var token = CreateNewDurationCtk();
            try
            {
                await Task.Delay(duration.Value, token).ConfigureAwait(false);
                session.Remove();
            }
            catch { }
        }
    }

}
