// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Globalization;
using System.Threading;

namespace Microsoft.CmdPal.Ext.Weather.Pages;

internal sealed partial class WeatherBandCard : ContentPage, IDisposable
{
	private readonly OpenMeteoService _weatherService;
	private readonly GeocodingService _geocodingService;
	private readonly WeatherSettingsManager _settings;
	private readonly FormContent _weatherForm = new();
	private readonly CancellationTokenSource _cts = new();

	public WeatherBandCard(
		OpenMeteoService weatherService,
		GeocodingService geocodingService,
		WeatherSettingsManager settings)
	{
		_weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
		_geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
		_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		Id = "com.baldbeardedbuilder.cmdpal.weather.card";
		Name = Resources.plugin_name;
		Title = Resources.plugin_name;
		Icon = Icons.WeatherIcon;

		_weatherForm.TemplateJson = GetCardTemplate();
		_weatherForm.DataJson = GetLoadingData();

		_settings.Settings.SettingsChanged += OnSettingsChanged;

		_ = LoadWeatherDataAsync();
	}

	public override IContent[] GetContent() => [_weatherForm];

	internal async Task LoadWeatherDataAsync()
	{
		try
		{
			var locations = await _geocodingService.SearchLocationAsync(
				_settings.DefaultLocation, _cts.Token);

			if (locations.Count == 0)
			{
				_weatherForm.DataJson = GetErrorData(
					Resources.location_not_found);
				RaiseItemsChanged();
				return;
			}

			var location = locations[0];
			var weather = await _weatherService.GetCurrentWeatherAsync(
				location.Latitude,
				location.Longitude,
				_settings.TemperatureUnit,
				_settings.WindSpeedUnit,
				_cts.Token);

			var forecast = await _weatherService.GetForecastAsync(
				location.Latitude,
				location.Longitude,
				_settings.TemperatureUnit,
				_cts.Token);

			_weatherForm.DataJson = BuildWeatherData(location, weather, forecast);
			RaiseItemsChanged();
		}
		catch (Exception ex)
		{
			ExtensionHost.LogMessage(new LogMessage
			{
				Message = $"Content page weather load error: {ex.Message}",
			});

			_weatherForm.DataJson = GetErrorData(
				Resources.unavailable);
			RaiseItemsChanged();
		}
	}

	private string BuildWeatherData(
		GeocodingResult location,
		WeatherData? weather,
		ForecastData? forecast)
	{
		var tempUnit = _settings.TemperatureUnit == "celsius" ? "\u00B0C" : "\u00B0F";
		var windUnit = _settings.WindSpeedUnit == "mph" ? "mph" : "km/h";

		var currentTemp = "--";
		var currentCondition = "Unknown";
		var currentIcon = "\uD83C\uDF24\uFE0F";
		var feelsLike = "--";
		var humidity = "--";
		var wind = "--";
		var todayHighLow = "--";

		if (weather?.Current != null)
		{
			var c = weather.Current;
			currentTemp = $"{c.Temperature:F0}{tempUnit}";
			currentCondition = Icons.GetWeatherDescription(c.WeatherCode);
			currentIcon = GetEmojiForWeatherCode(c.WeatherCode);
			feelsLike = $"{c.ApparentTemperature:F0}{tempUnit}";
			humidity = $"{c.RelativeHumidity}%";
			wind = $"{c.WindSpeed:F1} {windUnit}";
		}

		if (forecast?.Daily?.TemperatureMax?.Count > 0 &&
			forecast.Daily.TemperatureMin?.Count > 0)
		{
			var high = forecast.Daily.TemperatureMax[0];
			var low = forecast.Daily.TemperatureMin[0];
			todayHighLow = $"{high:F0}{tempUnit} / {low:F0}{tempUnit}";
		}

		var day1Name = "--";
		var day1Icon = currentIcon;
		var day1Condition = "--";
		var day1HighLow = "--";
		var day2Name = "--";
		var day2Icon = currentIcon;
		var day2Condition = "--";
		var day2HighLow = "--";
		var day3Name = "--";
		var day3Icon = currentIcon;
		var day3Condition = "--";
		var day3HighLow = "--";

		if (forecast?.Daily != null && forecast.Daily.Time?.Count >= 4)
		{
			for (var i = 1; i <= 3; i++)
			{
				if (forecast.Daily.Time == null || i >= forecast.Daily.Time.Count)
				{
					break;
				}

				var dateStr = forecast.Daily.Time[i];
				if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
				{
					var dayName = date.ToString("ddd", CultureInfo.CurrentCulture);
					var weatherCode = forecast.Daily.WeatherCode?[i] ?? 0;
					var icon = GetEmojiForWeatherCode(weatherCode);
					var condition = Icons.GetWeatherDescription(weatherCode);
					var high = forecast.Daily.TemperatureMax?[i] ?? 0;
					var low = forecast.Daily.TemperatureMin?[i] ?? 0;
					var highLow = $"{high:F0}{tempUnit} / {low:F0}{tempUnit}";

					if (i == 1)
					{
						day1Name = dayName;
						day1Icon = icon;
						day1Condition = condition;
						day1HighLow = highLow;
					}
					else if (i == 2)
					{
						day2Name = dayName;
						day2Icon = icon;
						day2Condition = condition;
						day2HighLow = highLow;
					}
					else if (i == 3)
					{
						day3Name = dayName;
						day3Icon = icon;
						day3Condition = condition;
						day3HighLow = highLow;
					}
				}
			}
		}

		var locationName = location.DisplayName;

		return $$"""
        {
            "locationName": "{{locationName}}",
            "currentIcon": "{{currentIcon}}",
            "currentTemp": "{{currentTemp}}",
            "currentCondition": "{{currentCondition}}",
            "feelsLike": "{{feelsLike}}",
            "todayHighLow": "{{todayHighLow}}",
            "humidity": "{{humidity}}",
            "wind": "{{wind}}",
            "day1Name": "{{day1Name}}",
            "day1Icon": "{{day1Icon}}",
            "day1Condition": "{{day1Condition}}",
            "day1HighLow": "{{day1HighLow}}",
            "day2Name": "{{day2Name}}",
            "day2Icon": "{{day2Icon}}",
            "day2Condition": "{{day2Condition}}",
            "day2HighLow": "{{day2HighLow}}",
            "day3Name": "{{day3Name}}",
            "day3Icon": "{{day3Icon}}",
            "day3Condition": "{{day3Condition}}",
            "day3HighLow": "{{day3HighLow}}"
        }
        """;
	}

	private static string GetEmojiForWeatherCode(int weatherCode)
	{
		return weatherCode switch
		{
			0 => "\u2600\uFE0F",
			1 or 2 => "\uD83C\uDF24\uFE0F",
			3 => "\u26C5",
			45 or 48 => "\uD83C\uDF2B\uFE0F",
			51 or 53 or 55 => "\uD83C\uDF26\uFE0F",
			56 or 57 => "\uD83C\uDF27\uFE0F",
			61 or 63 or 65 => "\uD83C\uDF27\uFE0F",
			66 or 67 => "\uD83C\uDF27\uFE0F",
			71 or 73 or 75 or 77 => "\u2744\uFE0F",
			80 or 81 or 82 => "\uD83C\uDF26\uFE0F",
			85 or 86 => "\uD83C\uDF28\uFE0F",
			95 => "\u26C8\uFE0F",
			96 or 99 => "\u26C8\uFE0F",
			_ => "\uD83C\uDF24\uFE0F",
		};
	}

	private static string GetLoadingData()
	{
		return """
        {
            "locationName": "Loading...",
            "currentIcon": "🌤️",
            "currentTemp": "--",
            "currentCondition": "Loading weather data...",
            "feelsLike": "--",
            "todayHighLow": "--",
            "humidity": "--",
            "wind": "--",
            "day1Name": "--",
            "day1Icon": "🌤️",
            "day1Condition": "--",
            "day1HighLow": "--",
            "day2Name": "--",
            "day2Icon": "🌤️",
            "day2Condition": "--",
            "day2HighLow": "--",
            "day3Name": "--",
            "day3Icon": "🌤️",
            "day3Condition": "--",
            "day3HighLow": "--"
        }
        """;
	}

	private static string GetErrorData(string errorMessage)
	{
		return $$"""
        {
            "locationName": "{{errorMessage}}",
            "currentIcon": "⚠️",
            "currentTemp": "--",
            "currentCondition": "{{errorMessage}}",
            "feelsLike": "--",
            "todayHighLow": "--",
            "humidity": "--",
            "wind": "--",
            "day1Name": "--",
            "day1Icon": "🌤️",
            "day1Condition": "--",
            "day1HighLow": "--",
            "day2Name": "--",
            "day2Icon": "🌤️",
            "day2Condition": "--",
            "day2HighLow": "--",
            "day3Name": "--",
            "day3Icon": "🌤️",
            "day3Condition": "--",
            "day3HighLow": "--"
        }
        """;
	}

	private static string GetCardTemplate()
	{
		return """
        {
            "type": "AdaptiveCard",
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "${locationName}",
                    "size": "large",
                    "weight": "bolder"
                },
                {
                    "type": "ColumnSet",
                    "style": "emphasis",
                    "columns": [
                        {
                            "type": "Column",
                            "width": "auto",
                            "verticalContentAlignment": "center",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "${currentIcon}",
                                    "size": "extraLarge",
                                    "horizontalAlignment": "center"
                                },
                                {
                                    "type": "TextBlock",
                                    "text": "${currentTemp}",
                                    "size": "extraLarge",
                                    "weight": "bolder",
                                    "horizontalAlignment": "center",
                                    "spacing": "small"
                                },
                                {
                                    "type": "TextBlock",
                                    "text": "${currentCondition}",
                                    "horizontalAlignment": "center",
                                    "wrap": true,
                                    "spacing": "small"
                                }
                            ]
                        },
                        {
                            "type": "Column",
                            "width": "stretch",
                            "verticalContentAlignment": "center",
                            "items": [
                                {
                                    "type": "FactSet",
                                    "facts": [
                                        { "title": "Feels like", "value": "${feelsLike}" },
                                        { "title": "High / Low", "value": "${todayHighLow}" },
                                        { "title": "Humidity", "value": "${humidity}" },
                                        { "title": "Wind", "value": "${wind}" }
                                    ]
                                }
                            ]
                        }
                    ]
                },
                {
                    "type": "TextBlock",
                    "text": "3-Day Forecast",
                    "size": "large",
                    "weight": "bolder",
                    "separator": true,
                    "spacing": "large"
                },
                {
                    "type": "ColumnSet",
                    "columns": [
                        {
                            "type": "Column",
                            "width": "stretch",
                            "style": "emphasis",
                            "verticalContentAlignment": "top",
                            "items": [
                                { "type": "TextBlock", "text": "${day1Name}", "weight": "bolder", "horizontalAlignment": "center" },
                                { "type": "TextBlock", "text": "${day1Icon}", "horizontalAlignment": "center", "size": "large", "spacing": "small" },
                                { "type": "TextBlock", "text": "${day1Condition}", "horizontalAlignment": "center", "wrap": true, "size": "small", "spacing": "small" },
                                { "type": "TextBlock", "text": "${day1HighLow}", "horizontalAlignment": "center", "size": "small", "spacing": "small" }
                            ]
                        },
                        {
                            "type": "Column",
                            "width": "stretch",
                            "style": "emphasis",
                            "spacing": "medium",
                            "verticalContentAlignment": "top",
                            "items": [
                                { "type": "TextBlock", "text": "${day2Name}", "weight": "bolder", "horizontalAlignment": "center" },
                                { "type": "TextBlock", "text": "${day2Icon}", "horizontalAlignment": "center", "size": "large", "spacing": "small" },
                                { "type": "TextBlock", "text": "${day2Condition}", "horizontalAlignment": "center", "wrap": true, "size": "small", "spacing": "small" },
                                { "type": "TextBlock", "text": "${day2HighLow}", "horizontalAlignment": "center", "size": "small", "spacing": "small" }
                            ]
                        },
                        {
                            "type": "Column",
                            "width": "stretch",
                            "style": "emphasis",
                            "spacing": "medium",
                            "verticalContentAlignment": "top",
                            "items": [
                                { "type": "TextBlock", "text": "${day3Name}", "weight": "bolder", "horizontalAlignment": "center" },
                                { "type": "TextBlock", "text": "${day3Icon}", "horizontalAlignment": "center", "size": "large", "spacing": "small" },
                                { "type": "TextBlock", "text": "${day3Condition}", "horizontalAlignment": "center", "wrap": true, "size": "small", "spacing": "small" },
                                { "type": "TextBlock", "text": "${day3HighLow}", "horizontalAlignment": "center", "size": "small", "spacing": "small" }
                            ]
                        }
                    ]
                }
            ]
        }
        """;
	}

	private async void OnSettingsChanged(object sender, Settings args)
	{
		await LoadWeatherDataAsync();
	}

	public void Dispose()
	{
		_settings.Settings.SettingsChanged -= OnSettingsChanged;
		_cts?.Cancel();
		_cts?.Dispose();
	}
}
