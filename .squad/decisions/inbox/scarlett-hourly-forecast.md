# Hourly Forecast Implementation — Architecture and Navigation Pattern

**Author:** Scarlett (Core Dev)
**Date:** 2026-03-02
**Status:** ✅ Implemented

## Summary

Implemented hourly forecast feature with data model, API integration, dedicated list page, and navigation from current weather item. Follows existing extension patterns for caching, page navigation, and JSON serialization.

## Implementation Decisions

| Decision | Rationale |
|----------|-----------|
| Hourly forecast in separate ForecastData.cs file | Keeps all forecast models together; follows existing pattern (DailyForecast already in same file) |
| 15-minute cache expiration | Matches existing GetCurrentWeatherAsync and GetForecastAsync cache strategy |
| `forecast_days=1` parameter | Only shows today's remaining hours; keeps data minimal and relevant |
| Page as command pattern | Pages can act as commands when passed to ListItem constructor; cleaner than separate ViewHourlyCommand |
| Filter past hours with `time < DateTime.Now` | Only shows future/current hours; prevents showing stale data for hours that have passed |
| HourlyForecastPage ID: `{prefix}.hourly.{location.Id}` | Unique per location; follows existing WeatherDetailPage pattern |

## Navigation Pattern Discovery

Initially created `ViewHourlyCommand` as an `InvokableCommand`, but discovered that pages can be passed directly to `ListItem()` constructor:

```csharp
// Pattern from WeatherListPage.cs
var detailPage = new WeatherDetailPage(...);
var item = new ListItem(detailPage) { ... };
```

This pattern:
- Eliminates need for separate command class
- Automatically handles navigation via page's `Id` property
- Cleaner code with less indirection
- Follows SDK's intended design (pages implement ICommand)

The `ViewHourlyCommand` file was created but isn't used. Could be removed in cleanup, but left for now in case explicit command is needed later.

## API Surface

**New Interface Method:**
```csharp
Task<HourlyForecastData?> GetHourlyForecastAsync(
    double latitude, 
    double longitude, 
    string temperatureUnit = "celsius", 
    string windSpeedUnit = "kmh", 
    CancellationToken ct = default);
```

**Open-Meteo Endpoint:**
```
GET https://api.open-meteo.com/v1/forecast
  ?latitude={lat}
  &longitude={lon}
  &hourly=temperature_2m,apparent_temperature,weather_code,precipitation_probability,wind_speed_10m,relative_humidity_2m
  &temperature_unit={unit}
  &wind_speed_unit={windUnit}
  &forecast_days=1
  &timezone=auto
```

## Data Model

**HourlyForecastData:**
- `Hourly` (HourlyForecast) — hourly data arrays
- `Latitude`, `Longitude`, `Timezone` — location metadata

**HourlyForecast:**
- `Time` (List<string>) — ISO 8601 timestamps
- `Temperature` (List<double>) — temperature_2m
- `ApparentTemperature` (List<double>) — feels-like temperature
- `WeatherCode` (List<int>) — WMO weather codes
- `PrecipitationProbability` (List<int>) — chance of rain (%)
- `WindSpeed` (List<double>) — wind_speed_10m
- `RelativeHumidity` (List<int>) — relative_humidity_2m

All properties follow Open-Meteo's JSON field naming via `[JsonPropertyName]` attributes.

## Testing Notes

**Manual Testing Required:**
1. Navigate to weather detail page
2. Select current weather item
3. Verify hourly forecast page opens
4. Verify only future/current hours are shown
5. Verify all data fields display correctly (temp, feels like, precipitation, wind, humidity)
6. Verify weather icons match conditions
7. Test with different temperature units (Celsius/Fahrenheit)
8. Test with different wind speed units (km/h, mph)

**Unit Tests:** Not included in this implementation. Snake Eyes can add tests for:
- HourlyForecast model deserialization
- GetHourlyForecastAsync caching behavior
- HourlyForecastPage time filtering logic

## Future Enhancements

Potential improvements (out of scope for this task):
- Add "Show More" to display full 24-hour forecast (next day hours)
- Add hourly precipitation chart/graph
- Add temperature trend visualization
- Add "Compare" feature to show hourly vs current weather
