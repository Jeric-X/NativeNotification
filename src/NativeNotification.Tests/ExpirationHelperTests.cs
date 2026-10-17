using Microsoft.VisualStudio.TestTools.UnitTesting;
using NativeNotification.Common;
using NativeNotification.Interface;

namespace NativeNotification.Tests;

[TestClass]
public class ExpirationHelperTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ReplacingDurationCancelsPreviousExpiration(bool useOptions)
    {
        var notification = new TestNotification();
        var helper = new ExpirationHelper(notification);
        SetDuration(helper, TimeSpan.FromMilliseconds(100), useOptions);
        SetDuration(helper, Timeout.InfiniteTimeSpan, useOptions);

        await Task.Delay(300, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(0, notification.RemoveCount);
        SetDuration(helper, null, useOptions);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ReplacementDurationStillExpires(bool useOptions)
    {
        var notification = new TestNotification();
        var helper = new ExpirationHelper(notification);
        SetDuration(helper, TimeSpan.FromMilliseconds(500), useOptions);
        SetDuration(helper, TimeSpan.FromMilliseconds(50), useOptions);

        await notification.Removed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationTokenSource.Token);
        await Task.Delay(600, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(1, notification.RemoveCount);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task DisablingExpirationCancelsPreviousDuration(bool useOptions)
    {
        var notification = new TestNotification();
        var helper = new ExpirationHelper(notification);
        SetDuration(helper, TimeSpan.FromMilliseconds(100), useOptions);
        SetDuration(helper, null, useOptions);

        await Task.Delay(300, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(0, notification.RemoveCount);
    }

    private static void SetDuration(ExpirationHelper helper, TimeSpan? duration, bool useOptions)
    {
        if (useOptions)
        {
            helper.SetNoficifationDuration(new NotificationDeliverOption { Duration = duration });
        }
        else
        {
            helper.SetNoficifationDuration(duration);
        }
    }

    private sealed class TestNotification : INotification
    {
        private int _removeCount;
        public int RemoveCount => Volatile.Read(ref _removeCount);
        public TaskCompletionSource Removed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string? Title { get; set; }
        public string? Message { get; set; }
        public Uri? Image { get; set; }
        public List<ActionButton> Buttons { get; set; } = [];
        public Action? ContentAction { get; set; }
        public string NotificationId => "test";
        public bool IsAlive => RemoveCount == 0;
        public bool IsCreatedByCurrentProcess => true;
        public void Show(NotificationDeliverOption? config = null) => throw new NotSupportedException();
        public bool Update() => throw new NotSupportedException();

        public void Remove()
        {
            Interlocked.Increment(ref _removeCount);
            Removed.TrySetResult();
        }
    }
}
