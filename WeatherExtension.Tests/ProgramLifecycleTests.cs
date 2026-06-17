// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class ProgramLifecycleTests
{
    [TestMethod]
    public void WaitHandle_WaitAnyReturnsImmediatelyWhenFirstHandleSignaled()
    {
        // Arrange
        using var handle1 = new ManualResetEvent(true); // Pre-signaled
        using var handle2 = new ManualResetEvent(false);

        // Act
        int result = WaitHandle.WaitAny(new[] { handle1, handle2 }, timeout: 100);

        // Assert
        Assert.AreEqual(0, result, "WaitAny should return index 0 when first handle is signaled");
    }

    [TestMethod]
    public void WaitHandle_WaitAnyReturnsSecondHandleIndex()
    {
        // Arrange
        using var handle1 = new ManualResetEvent(false);
        using var handle2 = new ManualResetEvent(true); // Pre-signaled

        // Act
        int result = WaitHandle.WaitAny(new[] { handle1, handle2 }, timeout: 100);

        // Assert
        Assert.AreEqual(1, result, "WaitAny should return index 1 when second handle is signaled");
    }

    [TestMethod]
    public void WaitHandle_WaitAnyTimesOutWhenNeitherSignaled()
    {
        // Arrange
        using var handle1 = new ManualResetEvent(false);
        using var handle2 = new ManualResetEvent(false);

        // Act
        int result = WaitHandle.WaitAny(new[] { handle1, handle2 }, timeout: 100);

        // Assert
        Assert.AreEqual(WaitHandle.WaitTimeout, result, "WaitAny should timeout when neither handle is signaled");
    }

    [TestMethod]
    public void CoordinatorAndDisposalEventCanBeWaitedTogether()
    {
        // Arrange
        using var disposalEvent = new ManualResetEvent(false);
        using var coordinator = new WeatherExtension.ShutdownCoordinator();
        var waitHandles = new[] { disposalEvent, coordinator.ShutdownHandle };

        // Act - Signal the disposal event
        disposalEvent.Set();
        int result = WaitHandle.WaitAny(waitHandles, timeout: 1000);

        // Assert
        Assert.AreEqual(0, result, "WaitAny should return 0 when disposal event is signaled first");
    }

    [TestMethod]
    public void TimerWatchdogCanBeDisabledAfterShutdownCompletes()
    {
        // Arrange
        var watchdogFired = false;
        Timer? watchdog = null;

        // Act
        watchdog = new Timer(
            _ => watchdogFired = true,
            state: null,
            dueTime: TimeSpan.FromMilliseconds(100),
            period: Timeout.InfiniteTimeSpan);

        // Simulate shutdown completing before timeout
        Thread.Sleep(50);
        watchdog.Change(Timeout.Infinite, Timeout.Infinite);
        Thread.Sleep(100);

        // Assert
        Assert.IsFalse(watchdogFired, "Watchdog should not fire if disabled before timeout");

        watchdog.Dispose();
    }

    [TestMethod]
    public void TimerWatchdogFiresIfShutdownSlow()
    {
        // Arrange
        var watchdogFired = false;
        Timer? watchdog = null;

        // Act
        watchdog = new Timer(
            _ => watchdogFired = true,
            state: null,
            dueTime: TimeSpan.FromMilliseconds(100),
            period: Timeout.InfiniteTimeSpan);

        // Simulate slow shutdown - don't disable the watchdog
        Thread.Sleep(200);

        // Assert
        Assert.IsTrue(watchdogFired, "Watchdog should fire if shutdown takes too long");

        watchdog.Dispose();
    }

    [TestMethod]
    public void ExtensionDisposeEventCanBeTriggeredManually()
    {
        // Arrange
        using var disposalEvent = new ManualResetEvent(false);
        var extension = new WeatherExtension.WeatherExtension(disposalEvent);

        // Act - Simulate external shutdown signal triggering manual Dispose
        extension.Dispose();

        // Assert
        Assert.IsTrue(disposalEvent.WaitOne(timeout: 100), "Manual Dispose should signal the event");
    }

    [TestMethod]
    public void MultipleHandlesAreThreadSafeInWaitAny()
    {
        // Arrange
        using var handle1 = new ManualResetEvent(false);
        using var handle2 = new ManualResetEvent(false);
        var results = new List<int>();

        // Act - Multiple threads waiting on same handles
        var threads = Enumerable.Range(0, 5).Select(_ => new Thread(() =>
        {
            int result = WaitHandle.WaitAny(new[] { handle1, handle2 }, timeout: 500);
            lock (results)
            {
                results.Add(result);
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        Thread.Sleep(100);

        // Signal handle1
        handle1.Set();

        threads.ForEach(t => t.Join(timeout: 2000));

        // Assert
        Assert.IsTrue(results.All(r => r == 0), "All waiting threads should return handle1's index");
    }
}
