// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Microsoft.CmdPal.Ext.Weather.Models;

namespace Microsoft.CmdPal.Ext.Weather.Services;

public interface IWeatherService
{
    Task<WeatherData?> GetCurrentWeatherAsync(double latitude, double longitude, string temperatureUnit = "celsius", string windSpeedUnit = "kmh", CancellationToken ct = default);

    Task<ForecastData?> GetForecastAsync(double latitude, double longitude, string temperatureUnit = "celsius", CancellationToken ct = default);

    Task<HourlyForecastData?> GetHourlyForecastAsync(double latitude, double longitude, string temperatureUnit = "celsius", string windSpeedUnit = "kmh", CancellationToken ct = default);
}
