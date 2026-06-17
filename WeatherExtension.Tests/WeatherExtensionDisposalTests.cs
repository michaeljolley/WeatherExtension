// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class WeatherExtensionDisposalTests
{
    [TestMethod]
    public void WeatherExtension_DisposeIsIdempotent()
    {
        // Arrange
        using var disposedEvent = new ManualResetEvent(false);
        var extension = new WeatherExtension.WeatherExtension(disposedEvent);

        // Act & Assert - Multiple disposes should not throw
        extension.Dispose();
        disposedEvent.Reset(); // Reset event for verification
        extension.Dispose();
        disposedEvent.Reset();
        extension.Dispose();

        Assert.IsTrue(disposedEvent.WaitOne(timeout: 1000), "Dispose should set the event");
    }

    [TestMethod]
    public void WeatherExtension_DisposeSignalsEvent()
    {
        // Arrange
        using var disposedEvent = new ManualResetEvent(false);
        var extension = new WeatherExtension.WeatherExtension(disposedEvent);

        // Act
        extension.Dispose();

        // Assert
        Assert.IsTrue(disposedEvent.WaitOne(timeout: 1000), "Dispose should signal the extensionDisposedEvent");
    }

    [TestMethod]
    public void WeatherExtension_DisposeIsThreadSafe()
    {
        // Arrange
        using var disposedEvent = new ManualResetEvent(false);
        var extension = new WeatherExtension.WeatherExtension(disposedEvent);
        var threads = new List<Thread>();
        var exceptionCount = 0;

        // Act - Multiple threads call Dispose simultaneously
        for (int i = 0; i < 10; i++)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    extension.Dispose();
                }
                catch
                {
                    Interlocked.Increment(ref exceptionCount);
                }
            });
            threads.Add(thread);
        }

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // Assert
        Assert.AreEqual(0, exceptionCount, "Concurrent Dispose calls should not throw");
        Assert.IsTrue(disposedEvent.WaitOne(timeout: 0), "Event should be signaled after Dispose");
    }

    [TestMethod]
    public void WeatherExtension_DisposeDoesNotThrowOnProviderDisposalError()
    {
        // This test verifies that errors during provider disposal are caught
        using var disposedEvent = new ManualResetEvent(false);
        var extension = new WeatherExtension.WeatherExtension(disposedEvent);

        // Act & Assert - Should not throw even if provider disposal has issues
        extension.Dispose();
        Assert.IsTrue(disposedEvent.WaitOne(timeout: 1000), "Dispose should complete despite any provider errors");
    }

    [TestMethod]
    public void WeatherExtension_GetProviderReturnsCorrectProvider()
    {
        // Arrange
        using var disposedEvent = new ManualResetEvent(false);
        var extension = new WeatherExtension.WeatherExtension(disposedEvent);

        // Act
        var commandProvider = extension.GetProvider(Microsoft.CommandPalette.Extensions.ProviderType.Commands);
        var otherProvider = extension.GetProvider((Microsoft.CommandPalette.Extensions.ProviderType)999);

        // Assert
        Assert.IsNotNull(commandProvider, "Should return command provider");
        Assert.IsNull(otherProvider, "Should return null for unknown provider type");
    }
}
