// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class WeatherDataModelTests
{
    [TestMethod]
    public void WeatherData_DeserializeRealCurrentWeatherResponse_Success()
    {
        // Real Open-Meteo current weather JSON response
        var json = """
        {
            "latitude": 47.6062,
            "longitude": -122.3321,
            "timezone": "America/Los_Angeles",
            "current": {
                "time": "2024-01-15T12:00",
                "temperature_2m": 10.5,
                "relative_humidity_2m": 75,
                "apparent_temperature": 8.2,
                "weather_code": 3,
                "wind_speed_10m": 12.5,
                "wind_direction_10m": 180
            }
        }
        """;

        var weatherData = JsonSerializer.Deserialize<WeatherData>(json, WeatherJsonContext.Default.WeatherData);

        Assert.IsNotNull(weatherData);
        Assert.AreEqual(47.6062, weatherData.Latitude);
        Assert.AreEqual(-122.3321, weatherData.Longitude);
        Assert.AreEqual("America/Los_Angeles", weatherData.Timezone);

        Assert.IsNotNull(weatherData.Current);
        Assert.AreEqual("2024-01-15T12:00", weatherData.Current.Time);
        Assert.AreEqual(10.5, weatherData.Current.Temperature);
        Assert.AreEqual(75, weatherData.Current.RelativeHumidity);
        Assert.AreEqual(8.2, weatherData.Current.ApparentTemperature);
        Assert.AreEqual(3, weatherData.Current.WeatherCode);
        Assert.AreEqual(12.5, weatherData.Current.WindSpeed);
        Assert.AreEqual(180, weatherData.Current.WindDirection);
    }

    [TestMethod]
    public void WeatherData_DeserializeWithNullCurrent_HandledGracefully()
    {
        var json = """
        {
            "latitude": 47.6062,
            "longitude": -122.3321,
            "timezone": "America/Los_Angeles"
        }
        """;

        var weatherData = JsonSerializer.Deserialize<WeatherData>(json, WeatherJsonContext.Default.WeatherData);

        Assert.IsNotNull(weatherData);
        Assert.AreEqual(47.6062, weatherData.Latitude);
        Assert.AreEqual(-122.3321, weatherData.Longitude);
        Assert.AreEqual("America/Los_Angeles", weatherData.Timezone);
        Assert.IsNull(weatherData.Current);
    }

    [TestMethod]
    public void ForecastData_DeserializeForecastResponse_Success()
    {
        // Real Open-Meteo forecast JSON response
        var json = """
        {
            "latitude": 47.6062,
            "longitude": -122.3321,
            "timezone": "America/Los_Angeles",
            "daily": {
                "time": ["2024-01-15", "2024-01-16", "2024-01-17"],
                "weather_code": [3, 61, 95],
                "temperature_2m_max": [12.5, 10.2, 8.7],
                "temperature_2m_min": [5.1, 3.8, 2.5],
                "precipitation_probability_max": [20, 80, 90]
            }
        }
        """;

        var forecastData = JsonSerializer.Deserialize<ForecastData>(json, WeatherJsonContext.Default.ForecastData);

        Assert.IsNotNull(forecastData);
        Assert.AreEqual(47.6062, forecastData.Latitude);
        Assert.AreEqual(-122.3321, forecastData.Longitude);
        Assert.AreEqual("America/Los_Angeles", forecastData.Timezone);

        Assert.IsNotNull(forecastData.Daily);
        Assert.IsNotNull(forecastData.Daily.Time);
        Assert.AreEqual(3, forecastData.Daily.Time.Count);
        Assert.AreEqual("2024-01-15", forecastData.Daily.Time[0]);
        Assert.AreEqual("2024-01-16", forecastData.Daily.Time[1]);
        Assert.AreEqual("2024-01-17", forecastData.Daily.Time[2]);

        Assert.IsNotNull(forecastData.Daily.WeatherCode);
        Assert.AreEqual(3, forecastData.Daily.WeatherCode.Count);
        Assert.AreEqual(3, forecastData.Daily.WeatherCode[0]);
        Assert.AreEqual(61, forecastData.Daily.WeatherCode[1]);
        Assert.AreEqual(95, forecastData.Daily.WeatherCode[2]);

        Assert.IsNotNull(forecastData.Daily.TemperatureMax);
        Assert.AreEqual(3, forecastData.Daily.TemperatureMax.Count);
        Assert.AreEqual(12.5, forecastData.Daily.TemperatureMax[0]);

        Assert.IsNotNull(forecastData.Daily.TemperatureMin);
        Assert.AreEqual(3, forecastData.Daily.TemperatureMin.Count);
        Assert.AreEqual(5.1, forecastData.Daily.TemperatureMin[0]);

        Assert.IsNotNull(forecastData.Daily.PrecipitationProbabilityMax);
        Assert.AreEqual(3, forecastData.Daily.PrecipitationProbabilityMax.Count);
        Assert.AreEqual(20, forecastData.Daily.PrecipitationProbabilityMax[0]);
    }

    [TestMethod]
    public void ForecastData_DeserializeWithNullDaily_HandledGracefully()
    {
        var json = """
        {
            "latitude": 47.6062,
            "longitude": -122.3321,
            "timezone": "America/Los_Angeles"
        }
        """;

        var forecastData = JsonSerializer.Deserialize<ForecastData>(json, WeatherJsonContext.Default.ForecastData);

        Assert.IsNotNull(forecastData);
        Assert.AreEqual(47.6062, forecastData.Latitude);
        Assert.AreEqual(-122.3321, forecastData.Longitude);
        Assert.AreEqual("America/Los_Angeles", forecastData.Timezone);
        Assert.IsNull(forecastData.Daily);
    }

    [TestMethod]
    public void GeocodingResponse_DeserializeRealResponse_Success()
    {
        var json = """
        {
            "results": [
                {
                    "id": 5809844,
                    "name": "Seattle",
                    "latitude": 47.6062,
                    "longitude": -122.3321,
                    "country": "United States",
                    "country_code": "US",
                    "admin1": "Washington",
                    "timezone": "America/Los_Angeles"
                }
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<GeocodingResponse>(json, WeatherJsonContext.Default.GeocodingResponse);

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Results);
        Assert.AreEqual(1, response.Results.Count);

        var result = response.Results[0];
        Assert.AreEqual(5809844, result.Id);
        Assert.AreEqual("Seattle", result.Name);
        Assert.AreEqual(47.6062, result.Latitude);
        Assert.AreEqual(-122.3321, result.Longitude);
        Assert.AreEqual("United States", result.Country);
        Assert.AreEqual("US", result.CountryCode);
        Assert.AreEqual("Washington", result.Admin1);
        Assert.AreEqual("America/Los_Angeles", result.Timezone);
    }

    [TestMethod]
    public void GeocodingResponse_DeserializeWithNullResults_HandledGracefully()
    {
        var json = "{}";

        var response = JsonSerializer.Deserialize<GeocodingResponse>(json, WeatherJsonContext.Default.GeocodingResponse);

        Assert.IsNotNull(response);
        Assert.IsNull(response.Results);
    }

    [TestMethod]
    public void WeatherData_DeserializeWithMissingFields_HandledGracefully()
    {
        var json = """
        {
            "latitude": 47.6062,
            "longitude": -122.3321
        }
        """;

        var weatherData = JsonSerializer.Deserialize<WeatherData>(json, WeatherJsonContext.Default.WeatherData);

        Assert.IsNotNull(weatherData);
        Assert.AreEqual(47.6062, weatherData.Latitude);
        Assert.AreEqual(-122.3321, weatherData.Longitude);
        Assert.IsNull(weatherData.Timezone);
        Assert.IsNull(weatherData.Current);
    }

    [TestMethod]
    public void CurrentWeather_DeserializeWithMissingFields_HandledGracefully()
    {
        var json = """
        {
            "latitude": 47.6062,
            "longitude": -122.3321,
            "current": {
                "time": "2024-01-15T12:00",
                "temperature_2m": 10.5
            }
        }
        """;

        var weatherData = JsonSerializer.Deserialize<WeatherData>(json, WeatherJsonContext.Default.WeatherData);

        Assert.IsNotNull(weatherData);
        Assert.IsNotNull(weatherData.Current);
        Assert.AreEqual("2024-01-15T12:00", weatherData.Current.Time);
        Assert.AreEqual(10.5, weatherData.Current.Temperature);
        Assert.AreEqual(0, weatherData.Current.RelativeHumidity);
        Assert.AreEqual(0, weatherData.Current.ApparentTemperature);
        Assert.AreEqual(0, weatherData.Current.WeatherCode);
        Assert.AreEqual(0, weatherData.Current.WindSpeed);
        Assert.AreEqual(0, weatherData.Current.WindDirection);
    }
}
