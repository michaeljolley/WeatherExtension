// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Microsoft.CmdPal.Ext.Weather.Models;

public sealed class ForecastData
{
    [JsonPropertyName("daily")]
    public DailyForecast? Daily { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}

#pragma warning disable SA1402 // File may only contain a single type

public sealed class DailyForecast
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double>? TemperatureMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double>? TemperatureMin { get; set; }

    [JsonPropertyName("precipitation_probability_max")]
    public List<int>? PrecipitationProbabilityMax { get; set; }
}

public sealed class HourlyForecastData
{
    [JsonPropertyName("hourly")]
    public HourlyForecast? Hourly { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}

public sealed class HourlyForecast
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public List<double>? Temperature { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public List<double>? ApparentTemperature { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }

    [JsonPropertyName("precipitation_probability")]
    public List<int>? PrecipitationProbability { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public List<double>? WindSpeed { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public List<int>? RelativeHumidity { get; set; }
}

#pragma warning restore SA1402 // File may only contain a single type
