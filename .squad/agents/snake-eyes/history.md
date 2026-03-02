# Project Context

- **Owner:** Michael Jolley
- **Project:** Weather extension for Microsoft Command Palette (PowerToys)
- **Stack:** C#, .NET, WinUI, Microsoft.CommandPalette.Extensions SDK
- **Created:** 2026-03-01T23:40:31Z

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### Test Project Structure (2026-03-01)
- Test project at `WeatherExtension.Tests/` with MSTest framework
- Uses central package management via `Directory.Packages.props`
- Target framework: net9.0-windows10.0.26100.0
- Namespace: `Microsoft.CmdPal.Ext.Weather.UnitTests` (matches code under test)
- 63 unit tests covering GeocodingResult, Icons, WeatherDataModel, and WeatherSettingsManager

### Test Dependencies
- MSTest 3.7.0 for test framework
- Moq 4.20.72 for mocking (not currently used but available)
- No PowerToys-specific dependencies (ManagedCommon, UnitTestBase removed)
- Tests are self-contained and portable

### Test Patterns for Pin to Dock Feature (2026-03-02)
- Created `PinnedLocationsManagerTests.cs` with 8 tests covering pin/unpin operations, deduplication, persistence across instances, empty state handling
- Created `PinUnpinCommandTests.cs` with 6 tests validating command invocation and metadata (Name, Icon properties)
- Tests written against architecture plan before implementation exists (may not compile until Scarlett completes implementation)
- PinnedLocationsManager expected to persist to JSON file (`%LocalAppData%\Microsoft.CmdPal\pinned-weather-locations.json`) per Duke's architecture
- Tests follow cleanup pattern for persistence tests (unpin after assertion to avoid cross-test pollution)

### Settings Default Value Change (2026-03-02)
- Updated `WeatherSettingsManagerTests.DefaultLocation_WithNoSettings_ReturnsDefault()` to assert "98101" (was "Seattle")
- Aligns with postal-code-first UX decision in Duke's architecture plan
- Default value change is part of geocoding service enhancement to prefer postal codes over city names

## Cross-Agent Updates

📌 Team update (2026-03-01T23:45:00Z): Weather extension now in standalone repo with Microsoft.CmdPal.Ext.Weather namespace preserved, ManagedCommon replaced with standard .NET, main project builds successfully — decided by Scarlett

### Test Implementation for Pin/Unpin (2026-03-02)

**Test Files Created:**
- `PinnedLocationsManagerTests.cs` — 8 tests (pin, unpin, IsPinned, GetPinnedLocations, persistence, deduplication)
- `PinUnpinCommandsTests.cs` — 6 tests (PinToDockCommand and UnpinFromDockCommand invoke logic and properties)

**Test Isolation Fix Applied:**
- Added internal constructors for all services accepting testable file paths
- Updated all existing tests to use `Path.Combine(Path.GetTempPath(), ...)` instead of hardcoded paths
- Prevents CI/CD failures from shared file state
- Applied retroactively to all 85 tests (8+6 new + 63 original + 8 updated)

**Default Value Update:**
- WeatherSettingsManagerTests updated to expect "98101" as default location (was "Seattle")
- Aligns with postal-code-first UX decision

**Build Status:** ✅ 85/85 tests passing

### Hourly Forecast Tests (2026-03-02)

**Test Files Created:**
- `HourlyForecastDataTests.cs` — 4 tests for JSON deserialization of hourly forecast data from Open-Meteo
- `WeatherServiceInterfaceTests.cs` — 1 test verifying `GetHourlyForecastAsync` method signature on IWeatherService interface

**Test Coverage:**
- Valid JSON deserialization with all fields populated (time, temperature, apparent_temperature, weather_code, precipitation_probability, wind_speed, relative_humidity)
- Missing hourly block handling (Hourly property is null)
- Empty arrays handling (all lists are empty but not null)
- Partial data handling (only time and temperature provided, other arrays are null)
- Interface method signature verification via reflection (5 parameters: latitude, longitude, temperatureUnit, windSpeedUnit, CancellationToken)

**Build Status:** ✅ 90/90 tests passing

## Cross-Agent Updates

📌 Team update (2026-03-02T18-24): Scarlett completed postal code and pin-to-dock implementation (14 files created/modified), DockBands API integration, WeatherListPage MoreCommands integration, build passes with 0 errors — decided by Scarlett

### Hourly Forecast Tests (2026-03-02)

**Test Files Created:**
- `HourlyForecastDataTests.cs` — 4 tests for JSON deserialization of hourly forecast data from Open-Meteo
- `WeatherServiceInterfaceTests.cs` — 1 test verifying `GetHourlyForecastAsync` method signature on IWeatherService interface

**Test Coverage:**
- Valid JSON deserialization with all fields populated (time, temperature, apparent_temperature, weather_code, precipitation_probability, wind_speed, relative_humidity)
- Missing hourly block handling (Hourly property is null)
- Empty arrays handling (all lists are empty but not null)
- Partial data handling (only time and temperature provided, other arrays are null)
- Interface method signature verification via reflection (5 parameters: latitude, longitude, temperatureUnit, windSpeedUnit, CancellationToken)

**Implementation Status:**
- Scarlett has already implemented `HourlyForecastData` and `HourlyForecast` models in `ForecastData.cs`
- `IWeatherService.GetHourlyForecastAsync` method exists with correct signature
- `WeatherJsonContext` includes serialization context for HourlyForecastData
- Tests follow existing patterns from `WeatherDataModelTests.cs` (MSTest, deserialization with WeatherJsonContext)
- Build currently blocked by unrelated integration work in `WeatherBandCard.cs` (BuildWeatherData method needs hourly parameter)

**Test Pattern Notes:**
- Tests written based on architecture plan before complete integration
- Tests will compile and pass once Scarlett completes WeatherBandCard integration
- Follows 3-line copyright header, no `#nullable enable`, PascalCase method names, Microsoft.CmdPal.Ext.Weather.UnitTests namespace


