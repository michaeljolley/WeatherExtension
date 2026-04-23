// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BaldBeardedBuilder.WeatherExtension;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CommandPalette.Extensions;
using System.Text.Json;

namespace Microsoft.CmdPal.Ext.Weather.Services;

public sealed partial class OpenMeteoService : IWeatherService, IDisposable
{
	private readonly HttpClient _httpClient;
	private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";
	private const int CacheExpirationMinutes = 15;

	private WeatherData? _cachedWeather;
	private DateTime _weatherCacheTime;
	private string _weatherCacheKey = string.Empty;

	private ForecastData? _cachedForecast;
	private DateTime _forecastCacheTime;
	private string _forecastCacheKey = string.Empty;

	private HourlyForecastData? _cachedHourly;
	private DateTime _hourlyCacheTime;
	private string _hourlyCacheKey = string.Empty;

	public OpenMeteoService()
	{
		_httpClient = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(10),
		};
		_httpClient.DefaultRequestHeaders.Add("User-Agent", "PowerToys-CmdPal-Weather/1.0");
	}

	public async Task<WeatherData?> GetCurrentWeatherAsync(
		double latitude,
		double longitude,
		string temperatureUnit = "celsius",
		string windSpeedUnit = "kmh",
		CancellationToken ct = default)
	{
		try
		{
			var cacheKey = $"{latitude},{longitude},{temperatureUnit},{windSpeedUnit}";
			if (_cachedWeather != null &&
				_weatherCacheKey == cacheKey &&
				(DateTime.UtcNow - _weatherCacheTime).TotalMinutes < CacheExpirationMinutes)
			{
				return _cachedWeather;
			}

			var url = $"{BaseUrl}?latitude={latitude}&longitude={longitude}" +
					 $"&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m,wind_direction_10m" +
					 $"&temperature_unit={temperatureUnit}&wind_speed_unit={windSpeedUnit}&timezone=auto";

			var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				WeatherLogger.LogToHost(
					MessageState.Error,
					$"Weather API returned status {response.StatusCode}");
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			var weatherData = JsonSerializer.Deserialize(content, WeatherJsonContext.Default.WeatherData);

			if (weatherData == null)
			{
				WeatherLogger.LogToHost(
					MessageState.Info,
					$"Weather deserialization returned null. Status: {response.StatusCode}, Content length: {content.Length}");
			}

			if (weatherData != null)
			{
				_cachedWeather = weatherData;
				_weatherCacheTime = DateTime.UtcNow;
				_weatherCacheKey = cacheKey;
			}

			return weatherData;
		}
		catch (Exception ex)
		{
			WeatherLogger.LogToHost(
				MessageState.Error,
				$"Weather fetch error: {ex.Message}");
			return null;
		}
	}

	public async Task<ForecastData?> GetForecastAsync(
		double latitude,
		double longitude,
		string temperatureUnit = "celsius",
		CancellationToken ct = default)
	{
		try
		{
			var cacheKey = $"{latitude},{longitude},{temperatureUnit}";
			if (_cachedForecast != null &&
				_forecastCacheKey == cacheKey &&
				(DateTime.UtcNow - _forecastCacheTime).TotalMinutes < CacheExpirationMinutes)
			{
				return _cachedForecast;
			}

			var url = $"{BaseUrl}?latitude={latitude}&longitude={longitude}" +
					 $"&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
					 $"&temperature_unit={temperatureUnit}&timezone=auto";

			var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				WeatherLogger.LogToHost(
					MessageState.Error,
					$"Forecast API returned status {response.StatusCode}");
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			var forecastData = JsonSerializer.Deserialize(content, WeatherJsonContext.Default.ForecastData);

			if (forecastData == null)
			{
				WeatherLogger.LogToHost(
					MessageState.Info,
					$"Forecast deserialization returned null. Status: {response.StatusCode}, Content length: {content.Length}");
			}

			if (forecastData != null)
			{
				_cachedForecast = forecastData;
				_forecastCacheTime = DateTime.UtcNow;
				_forecastCacheKey = cacheKey;
			}

			return forecastData;
		}
		catch (Exception ex)
		{
			WeatherLogger.LogToHost(
				MessageState.Error,
				$"Forecast fetch error: {ex.Message}");
			return null;
		}
	}

	public async Task<HourlyForecastData?> GetHourlyForecastAsync(
		double latitude,
		double longitude,
		string temperatureUnit = "celsius",
		string windSpeedUnit = "kmh",
		CancellationToken ct = default)
	{
		try
		{
			var cacheKey = $"{latitude},{longitude},{temperatureUnit},{windSpeedUnit}";
			if (_cachedHourly != null &&
				_hourlyCacheKey == cacheKey &&
				(DateTime.UtcNow - _hourlyCacheTime).TotalMinutes < CacheExpirationMinutes)
			{
				return _cachedHourly;
			}

			var url = $"{BaseUrl}?latitude={latitude}&longitude={longitude}" +
					 $"&hourly=temperature_2m,apparent_temperature,weather_code,precipitation_probability,wind_speed_10m,relative_humidity_2m" +
					 $"&temperature_unit={temperatureUnit}&wind_speed_unit={windSpeedUnit}&forecast_days=2&timezone=auto";

			var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				WeatherLogger.LogToHost(
					MessageState.Error,
					$"Hourly forecast API returned status {response.StatusCode}");
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			var hourlyData = JsonSerializer.Deserialize(content, WeatherJsonContext.Default.HourlyForecastData);

			if (hourlyData == null)
			{
				WeatherLogger.LogToHost(
					MessageState.Info,
					$"Hourly forecast deserialization returned null. Status: {response.StatusCode}, Content length: {content.Length}");
			}

			if (hourlyData != null)
			{
				_cachedHourly = hourlyData;
				_hourlyCacheTime = DateTime.UtcNow;
				_hourlyCacheKey = cacheKey;
			}

			return hourlyData;
		}
		catch (Exception ex)
		{
			WeatherLogger.LogToHost(
				MessageState.Error,
				$"Hourly forecast fetch error: {ex.Message}");
			return null;
		}
	}

	public void Dispose()
	{
		_httpClient?.Dispose();
	}
}
