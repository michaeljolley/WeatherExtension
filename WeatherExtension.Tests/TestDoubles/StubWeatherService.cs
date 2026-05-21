// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests.TestDoubles;

internal sealed class StubWeatherService : IWeatherService
{
	public int CurrentWeatherCallCount { get; private set; }

	public WeatherData Current { get; set; } = new()
	{
		Current = new CurrentWeather
		{
			Temperature = 20,
			ApparentTemperature = 19,
			RelativeHumidity = 55,
			WeatherCode = 0,
			WindSpeed = 10,
			WindDirection = 180,
		},
	};

	public Task<WeatherData?> GetCurrentWeatherAsync(
		double latitude,
		double longitude,
		string temperatureUnit = "celsius",
		string windSpeedUnit = "kmh",
		CancellationToken ct = default)
	{
		CurrentWeatherCallCount++;
		ct.ThrowIfCancellationRequested();
		return Task.FromResult<WeatherData?>(Current);
	}

	public Task<ForecastData?> GetForecastAsync(
		double latitude,
		double longitude,
		string temperatureUnit = "celsius",
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();
		return Task.FromResult<ForecastData?>(null);
	}

	public HourlyForecastData Hourly { get; set; } = CreateDefaultHourly();

	public Task<HourlyForecastData?> GetHourlyForecastAsync(
		double latitude,
		double longitude,
		string temperatureUnit = "celsius",
		string windSpeedUnit = "kmh",
		CancellationToken ct = default)
	{
		ct.ThrowIfCancellationRequested();
		return Task.FromResult<HourlyForecastData?>(Hourly);
	}

	private static HourlyForecastData CreateDefaultHourly()
	{
		var future = DateTime.Now.AddHours(1).ToString("O");
		return new HourlyForecastData
		{
			Hourly = new HourlyForecast
			{
				Time = [future],
				Temperature = [21],
				ApparentTemperature = [20],
				WeatherCode = [0],
				PrecipitationProbability = [10],
				WindSpeed = [5],
				RelativeHumidity = [60],
			},
		};
	}
}
