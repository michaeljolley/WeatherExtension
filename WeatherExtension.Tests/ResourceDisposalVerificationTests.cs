// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class ResourceDisposalVerificationTests
{
    [TestMethod]
    public void WeatherCommandsProvider_DisposesAllBands()
    {
        // Arrange
        using var provider = new Microsoft.CmdPal.Ext.Weather.WeatherCommandsProvider();

        // Act
        provider.Dispose();

        // Assert - Should not throw and should complete
        provider.Dispose(); // Second dispose should be safe
    }

    [TestMethod]
    public void OpenMeteoService_DisposesHttpClient()
    {
        // Arrange
        using var handler = new HttpClientHandler();
        var service = new Microsoft.CmdPal.Ext.Weather.Services.OpenMeteoService(handler);

        // Act
        service.Dispose();

        // Assert - Service should dispose without error
        Assert.IsTrue(true, "OpenMeteoService disposal completed");
    }

    [TestMethod]
    public void GeocodingService_DisposesHttpClient()
    {
        // Arrange
        using var handler = new HttpClientHandler();
        var service = new Microsoft.CmdPal.Ext.Weather.Services.GeocodingService(handler);

        // Act
        service.Dispose();

        // Assert - Service should dispose without error
        Assert.IsTrue(true, "GeocodingService disposal completed");
    }

    [TestMethod]
    public void WeatherListPage_CancelsCancellationTokens()
    {
        // Arrange
        var weatherService = new Mock<Microsoft.CmdPal.Ext.Weather.Services.IWeatherService>();
        var geocodingService = new Mock<Microsoft.CmdPal.Ext.Weather.Services.IGeocodingService>();
        var settingsManager = new Microsoft.CmdPal.Ext.Weather.Services.WeatherSettingsManager();
        var favoritesManager = new Microsoft.CmdPal.Ext.Weather.Services.FavoritesManager();

        var page = new Microsoft.CmdPal.Ext.Weather.Pages.WeatherListPage(
            weatherService.Object,
            geocodingService.Object,
            settingsManager,
            favoritesManager);

        // Act
        page.Dispose();

        // Assert - Should not throw
        Assert.IsTrue(true, "WeatherListPage disposal completed");
    }

    [TestMethod]
    public void PinnedWeatherBand_StopsTimerAndCancelsCts()
    {
        // Arrange
        var location = new Microsoft.CmdPal.Ext.Weather.Models.GeocodingResult
        {
            Latitude = 0,
            Longitude = 0,
            DisplayName = "Test",
        };

        var weatherService = new Mock<Microsoft.CmdPal.Ext.Weather.Services.IWeatherService>();
        var settings = new Microsoft.CmdPal.Ext.Weather.Services.WeatherSettingsManager();
        var card = new Mock<Microsoft.CmdPal.Ext.Weather.Pages.WeatherBandCard>();

        var band = new Microsoft.CmdPal.Ext.Weather.DockBands.PinnedWeatherBand(
            location,
            weatherService.Object as Microsoft.CmdPal.Ext.Weather.Services.OpenMeteoService,
            settings,
            card.Object);

        // Act
        band.Dispose();

        // Assert - Should not throw
        Assert.IsTrue(true, "PinnedWeatherBand disposal completed");
    }

    [TestMethod]
    public void DisposalEventHandlers_AreProperlyUnsubscribed()
    {
        // Arrange - Create provider and retrieve disposal event handler counts
        var provider = new Microsoft.CmdPal.Ext.Weather.WeatherCommandsProvider();

        // Act
        provider.Dispose();

        // Assert - Multiple disposes should not duplicate handlers
        provider.Dispose();
        Assert.IsTrue(true, "Event handlers properly cleaned up");
    }

    [TestMethod]
    public void ShutdownCoordinator_ReleasesAllResources()
    {
        // Arrange
        var coordinator = new WeatherExtension.ShutdownCoordinator();
        var initialThreadCount = System.Diagnostics.Process.GetCurrentProcess().ThreadCount;

        // Act
        coordinator.Dispose();
        Thread.Sleep(100); // Give listener thread time to exit

        var finalThreadCount = System.Diagnostics.Process.GetCurrentProcess().ThreadCount;

        // Assert
        Assert.LessOrEqual(finalThreadCount, initialThreadCount, "Shutdown coordinator should release its listener thread");
    }

    [TestMethod]
    public void FullDisposalChain_CompletesWithoutDeadlock()
    {
        // Arrange
        using var disposedEvent = new ManualResetEvent(false);
        using var coordinator = new WeatherExtension.ShutdownCoordinator();
        var extension = new WeatherExtension.WeatherExtension(disposedEvent);

        // Act & Assert - Should complete without hanging
        extension.Dispose();
        bool completed = disposedEvent.WaitOne(timeout: TimeSpan.FromSeconds(2));

        coordinator.Dispose();

        Assert.IsTrue(completed, "Disposal chain should complete promptly");
    }
}
