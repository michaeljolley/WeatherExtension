# Project Context

- **Owner:** Michael Jolley
- **Project:** Weather extension for Microsoft Command Palette (PowerToys)
- **Stack:** C#, .NET, WinUI, Microsoft.CommandPalette.Extensions SDK
- **Created:** 2026-03-01T23:40:31Z

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- **SDK version:** `0.9.260204002-experimental` — DockBands API (`GetDockBands()`, `WrappedDockItem`) is fully available and compiling. Previous decision noting it as unavailable is outdated.
- **Build command:** `dotnet build WeatherExtension\WeatherExtension.csproj -r win-x64` (requires RuntimeIdentifier due to MSIX packaging).
- **Geocoding API:** Open-Meteo geocoding (`geocoding-api.open-meteo.com/v1/search?name=...`) uses `name` parameter — does NOT natively support postal codes. Postal code support needs a secondary geocoder (e.g., Nominatim).
- **Settings architecture:** Settings at `%LocalAppData%\Microsoft.CmdPal\settings.json`. Pinned locations should go in a separate file to keep settings clean.
- **Resource strings:** Clean snake_case keys in `Properties/Resources.resx`. Three keys relevant to location input: `search_placeholder`, `default_location_placeholder`, `default_location_description`.
- **Default location:** Currently "Seattle" — changing to "98101" to match postal-code-first UX.
- **MoreCommands pattern:** `WeatherListPage.CreateWeatherItem()` attaches context menu commands via `MoreCommands` array on `ListItem`. This is where PinToDock command goes.
- **CurrentWeatherBand:** A `ListItem` that auto-refreshes on a timer from `_settings.DefaultLocation`. For pinned locations, need a parameterized variant (`PinnedWeatherBand`).

## Cross-Agent Updates

📌 Team update (2026-03-02T18-24): Postal code preference and pin-to-dock features completed — Scarlett implemented all components, Snake Eyes wrote 14 tests, 85/85 passing, architecture decision merged to decisions.md — decided by Scarlett & Snake Eyes

## Issue #14 Root Cause Analysis (2026-03-19)

**Problem:** "Weather unavailable" on fresh install despite API functionality
**Root Cause:** Missing `[JsonPropertyName("results")]` attribute on `GeocodingResponse.Results` property causes JSON deserialization to fail with source-generated serializers
**Key Findings:**
- Open-Meteo APIs are fully functional (verified via direct HTTP tests)
- Source generation with `PropertyNameCaseInsensitive = true` has limited reliability for case-insensitive matching
- Deserialization returns null → empty location list → "Weather unavailable" cascade
- Error logging exists but provides insufficient diagnostic context

**Architecture Decision:** Always use explicit `[JsonPropertyName]` attributes for all DTO properties when using source-generated JSON serializers, regardless of case-insensitivity settings. This ensures reliable deserialization across all compilation modes (AOT, JIT, trimmed).

**Fix Plan:**
1. Add `[JsonPropertyName("results")]` to `GeocodingResponse.Results` (P0)
2. Enhance error logging to log deserialization failures with content samples (P1)
3. Add null guards after deserialization calls (P1)
4. Differentiate error messages in UI ("Location not found" vs "Service unavailable") (P2)

**Testing Requirements:** Unit tests for JSON deserialization of all API response types, integration tests for end-to-end flow from geocoding to weather display.

**Blast Radius:** Low risk — single line fix to root cause, defensive improvements to error handling. No breaking changes.

## Issue #15 Root Cause Analysis (2026-03-20)

**Problem:** Weather band title/subtitle shows current weather, but clicking to expand shows stale/outdated data in the content page (WeatherBandCard).

**Root Cause:** Architecture timing mismatch between band and content page updates:
- `CurrentWeatherBand` and `PinnedWeatherBand` fetch weather on a timer (default 30min) and update their own Title/Subtitle properties
- `WeatherBandCard` (the expanded view) fetches data ONCE in its constructor and only refreshes on settings changes
- **No synchronization between band timer updates and card data**—band refreshes but never tells card to refresh
- Command Palette SDK has NO lifecycle methods (OnNavigatedTo, OnActivated) to trigger card refresh on expansion

**Key Findings:**
- Single shared `WeatherBandCard` instance created in `WeatherCommandsProvider` constructor (line 32)
- Band's `Command` property points to this card—expansion shows the same instance every time
- Band's `UpdateWeatherAsync()` updates band display but doesn't call `_contentPage.LoadWeatherDataAsync()`
- Card has proper async refresh logic (`LoadWeatherDataAsync`) but it's only invoked at startup + settings changes
- For pinned bands, each has its own dedicated `WeatherBandCard` instance with same timing issue

**Architecture Decision:** Band timer should refresh its associated content page to ensure data consistency. Accept slight performance overhead (duplicate API calls) for UX correctness. Future optimization: pass data from band to card to eliminate duplicate fetches.

**Fix Plan:**
1. In `CurrentWeatherBand.UpdateWeatherAsync()` after line 98: add `await _contentPage.LoadWeatherDataAsync();`
2. In `PinnedWeatherBand.UpdateWeatherAsync()` after line 86: add `await _contentPage.LoadWeatherDataAsync();`

**Files Affected:** 
- `WeatherExtension/DockBands/CurrentWeatherBand.cs`
- `WeatherExtension/DockBands/PinnedWeatherBand.cs`

**Testing Requirements:** Manual verification that expanded card shows same data as band title/subtitle after timer updates. Verify pinned bands work correctly. Regression test settings changes still trigger refresh.

**Blast Radius:** Low risk — two small focused changes, no API changes, existing error handling covers failure cases. Performance impact minimal (timer already fires every 30min).

## Issue #25 Root Cause Analysis (2026-03-20)

**Problem:** Hourly forecasts stop at midnight — command palette shows remaining hours today only; dock band shows "--" for hour slots when <3 hours remain in the day.

**Root Cause:** `OpenMeteoService.GetHourlyForecastAsync()` uses `forecast_days=1` in the API URL (line 180), so Open-Meteo only returns hours for the current calendar day (00:00–23:00). All downstream consumers (HourlyForecastPage, WeatherBandCard) correctly filter to future hours, but run out of data near end of day.

**Key Findings:**
- API layer is the single bottleneck — `forecast_days=1` caps data at today's hours
- Data models (`HourlyForecast` with `List<>` properties) can hold multi-day data without changes
- `HourlyForecastPage.CreateHourlyItems()` filters `time < now` (correct) but needs a 24-hour cap when API returns 2 days
- `WeatherBandCard.BuildWeatherData()` picks next 3 future hours (correct logic, just starved of data)
- Timezone note: `DateTime.Now` vs `timezone=auto` is a separate P2 concern for cross-timezone searches

**Fix Plan:**
1. Change `forecast_days=1` → `forecast_days=2` in `OpenMeteoService.cs` line 180 (P0)
2. Add 24-hour cap in `HourlyForecastPage.cs` after the past-hour filter (P0)
3. No changes needed in `WeatherBandCard.cs` or data models

**Blast Radius:** Very low — one API parameter, one guard clause. No model or breaking changes.
