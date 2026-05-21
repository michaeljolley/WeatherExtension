// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using BaldBeardedBuilder.WeatherExtension;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Pages;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.CmdPal.Ext.Weather.UnitTests.TestDoubles;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class HourlyForecastPageTests
{
	private string _settingsPath = string.Empty;

	[TestInitialize]
	public void Setup()
	{
		_settingsPath = Path.Combine(Path.GetTempPath(), $"weather-hourly-settings-{Guid.NewGuid()}.json");
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (File.Exists(_settingsPath))
		{
			File.Delete(_settingsPath);
		}
	}

	[TestMethod]
	public void Constructor_SetsPageIdFromLocation()
	{
		var location = new GeocodingResult
		{
			Name = "Istanbul",
			Latitude = 41.0082,
			Longitude = 28.9784,
		};

		using var page = new HourlyForecastPage(
			location,
			new StubWeatherService(),
			new WeatherSettingsManager(_settingsPath));

		StringAssert.Contains(page.Id, location.Id.ToString());
	}

	[TestMethod]
	public async Task GetItems_AfterHourlyDataLoaded_ReturnsRows()
	{
		var location = new GeocodingResult { Name = "Test", Latitude = 41, Longitude = 29 };
		var weather = new StubWeatherService();
		weather.Current = new WeatherData
		{
			Current = new CurrentWeather { Temperature = 22, WeatherCode = 0, WindSpeed = 5, WindDirection = 90 },
		};

		using var page = new HourlyForecastPage(
			location,
			weather,
			new WeatherSettingsManager(_settingsPath));

		var items = await AsyncTestHelper.WaitUntilAsync(
			() => page.GetItems(),
			items => items.Length > 0 && items[0].Title != Resources.loading_data,
			timeoutMs: 8000);

		Assert.IsTrue(items.Length > 0);
	}
}
