# Project Context

- **Owner:** Michael Jolley
- **Project:** Weather extension for Microsoft Command Palette (PowerToys)
- **Stack:** C#, .NET, WinUI, Microsoft.CommandPalette.Extensions SDK
- **Created:** 2026-03-01T23:40:31Z

## Learnings

### 2026-03-01: Weather Extension Migration from PowerToys

**Architecture:**
- Migrated full Weather extension from PowerToys repo (michaeljolley/PowerToys:dev/mjolley/weather-extension)
- Namespace: `Microsoft.CmdPal.Ext.Weather` (preserved from source, not renamed to WeatherExtension)
- RootNamespace in .csproj: `Microsoft.CmdPal.Ext.Weather`
- Entry point: WeatherExtension class in WeatherExtension namespace (for MSIX registration)

**Key Patterns:**
- **Settings Management:** JsonSettingsManager base class with custom settings path
  - Standalone implementation: `%LocalAppData%\Microsoft.CmdPal\settings.json`
  - Replaces PowerToys' ManagedCommon.Utilities.BaseSettingsPath()
- **Resource Strings:** EmbeddedResource with PublicResXFileCodeGenerator
  - Resources.resx → Resources.Designer.cs (auto-generated)
  - Namespace: Microsoft.CmdPal.Ext.Weather
- **JSON Serialization:** Source-generated JsonSerializerContext for AOT compatibility
  - WeatherJsonContext with JsonSerializable attributes
  - Trim-safe and AOT-compatible

**File Structure:**
```
WeatherExtension/
├── Commands/           (ChangeLocationCommand, RefreshWeatherCommand)
├── DockBands/          (CurrentWeatherBand)
├── Models/             (ForecastData, GeocodingResult, WeatherData)
├── Pages/              (WeatherContentPage, WeatherDetailPage, WeatherListPage)
├── Services/           (GeocodingService, OpenMeteoService, WeatherSettingsManager, etc.)
├── Properties/         (AssemblyInfo, Resources.resx, Resources.Designer.cs)
├── Assets/             (Weather.svg)
├── WeatherCommandsProvider.cs (main provider)
├── WeatherExtension.cs (MSIX entry point)
├── GlobalUsings.cs
└── Icons.cs
```

**API Surface:**
- CommandProvider.TopLevelCommands() - returns weather commands
- CommandProvider.GetDockBands() - commented out (may not be in current SDK)
- IWeatherService interface - implemented by OpenMeteoService
- Settings: TextSetting, ChoiceSetSetting, ToggleSetting

**Dependencies Removed:**
- ManagedCommon (PowerToys internal) - replaced with standard .NET
- Microsoft.CmdPal.Common (PowerToys internal) - not needed

**Build Status:** ✅ Compiles successfully (test project has accessibility issues with internal types)

### Resource Key Prefix Cleanup

**Decision:** Removed `Microsoft_plugin_weather_` prefix from all 36 resource keys in Resources.resx. This was a PowerToys naming convention no longer needed in the standalone extension.

**Files modified (8 total):**
- `WeatherExtension/Properties/Resources.resx` — 36 resource keys renamed (e.g., `Microsoft_plugin_weather_celsius` → `celsius`)
- `WeatherExtension/Properties/Resources.Designer.cs` — All auto-generated property names and GetString calls updated to match
- `WeatherExtension/WeatherCommandsProvider.cs` — 2 references updated
- `WeatherExtension/Services/WeatherSettingsManager.cs` — 20 references updated
- `WeatherExtension/Pages/WeatherContentPage.cs` — 4 references updated
- `WeatherExtension/Pages/WeatherDetailPage.cs` — 14 references updated
- `WeatherExtension/Pages/WeatherListPage.cs` — 4 references updated
- `WeatherExtension/DockBands/CurrentWeatherBand.cs` — 6 references updated

**Pattern:** Resource keys now use clean snake_case without namespace prefixes (e.g., `plugin_name`, `celsius`, `default_location_title`). Two pre-existing keys (`extension_name`, `extension_description`) already had clean names and were not touched.

**Build:** ✅ Passes with only pre-existing warnings (CA1824, CsWinRT1028, CA1859).

## Cross-Agent Updates

📌 Team update (2026-03-01T23:45:00Z): Test project now includes MSTest 3.7.0 + Moq 4.20.72 in central package management, 63 unit tests migrated with 68/71 passing — decided by Snake Eyes

📌 Team update (2026-03-02T00:18:00Z): Resource key prefix removed from all 36 keys across 8 source files, build passes, 68/71 tests pass (same baseline) — decided by Scarlett

### 2026-03-02: Postal Code Preference and Pin Location to Dock

**Features Implemented:**

1. **Postal Code Preference (Feature 1):**
   - Updated resource strings: `search_placeholder`, `default_location_placeholder`, `default_location_description`
   - Changed default location from "Seattle" to "98101" in WeatherSettingsManager (both default value and fallback)
   - Enhanced GeocodingService with postal code detection using regex patterns (US zip, Canadian postal, international)
   - Added Nominatim fallback API for postal code resolution when Open-Meteo fails
   - Created NominatimResult model and added to WeatherJsonContext for JSON serialization

2. **Pin Location to Dock (Feature 2):**
   - Created PinnedLocationsManager service managing pinned locations in `%LocalAppData%\Microsoft.CmdPal\pinned-weather-locations.json`
   - Created PinnedLocation model with ToGeocodingResult() conversion
   - Created PinToDockCommand and UnpinFromDockCommand as InvokableCommands (📌/📍 emojis)
   - Created PinnedWeatherBand dock band parameterized by specific location
   - Modified WeatherCommandsProvider to inject PinnedLocationsManager, handle PinnedLocationsChanged event, rebuild dock bands dynamically
   - Modified WeatherListPage to add pin/unpin commands to MoreCommands based on IsPinned() check

**Key Patterns:**
- **Postal Code Detection:** Three regex patterns for US (5-digit + optional 4), Canada (A1A 1A1), international (4-6 digits)
- **Nominatim Integration:** Free geocoding API for postal code fallback, no API key required
- **Pin State Management:** Location comparison uses 0.01 degree tolerance for lat/lon matching
- **Dock Band Lifecycle:** PinnedLocationsChanged event triggers disposal of old bands before rebuilding
- **Icon Pattern:** IconInfo uses string constructor for emoji icons (not Glyph property)

**Files Modified (4):**
- `WeatherExtension/Properties/Resources.resx` — 3 resource value changes
- `WeatherExtension/Services/WeatherSettingsManager.cs` — default value "Seattle" → "98101" (2 locations)
- `WeatherExtension/Services/GeocodingService.cs` — postal code detection + Nominatim fallback
- `WeatherExtension/Pages/WeatherListPage.cs` — constructor param + MoreCommands conditional logic

**Files Created (8):**
- `WeatherExtension/Models/NominatimResult.cs` — Nominatim API response model
- `WeatherExtension/Models/PinnedLocation.cs` — Persisted pinned location data
- `WeatherExtension/Services/PinnedLocationsManager.cs` — Pin/unpin/persist logic
- `WeatherExtension/Commands/PinToDockCommand.cs` — Pin location command
- `WeatherExtension/Commands/UnpinFromDockCommand.cs` — Unpin location command
- `WeatherExtension/DockBands/PinnedWeatherBand.cs` — Dock band for specific location
- `WeatherExtension/WeatherCommandsProvider.cs` — Modified to integrate PinnedLocationsManager
- `WeatherExtension/Services/WeatherJsonContext.cs` — Added List<NominatimResult> and List<PinnedLocation>

**Build Status:** ✅ Compiles successfully with 2 pre-existing warnings (NETSDK1198, KillRunningExecutable).

## Cross-Agent Updates

📌 Team update (2026-03-02T18-24): Snake Eyes completed 14 unit tests (8 PinnedLocationsManager, 6 Pin/UnpinCommands), fixed test isolation via temp file paths, all 85 tests passing — decided by Snake Eyes

### 2026-03-02: Hourly Forecast Implementation

**Features Implemented:**

1. **Hourly Forecast Data Model:**
   - Extended `Models/ForecastData.cs` with `HourlyForecastData` and `HourlyForecast` classes
   - Properties: time, temperature, apparent temperature, weather code, precipitation probability, wind speed, relative humidity
   - Registered `HourlyForecastData` in `WeatherJsonContext` for source-generated JSON serialization

2. **Hourly Forecast API:**
   - Added `GetHourlyForecastAsync()` method to `IWeatherService` interface
   - Implemented in `OpenMeteoService` with 15-minute caching (follows existing pattern)
   - API URL: `&hourly=temperature_2m,apparent_temperature,weather_code,precipitation_probability,wind_speed_10m,relative_humidity_2m&forecast_days=1`
   - Cache fields: `_cachedHourly`, `_hourlyCacheTime`, `_hourlyCacheKey`

3. **HourlyForecastPage:**
   - New `ListPage` at `Pages/HourlyForecastPage.cs`
   - Displays remaining hours from current time through end of day
   - Each item shows: formatted time (h:mm tt), condition, temperature
   - Details pane: temperature, feels like, precipitation chance, wind speed, humidity
   - Filters out past hours (only shows time >= DateTime.Now)

4. **ViewHourlyCommand:**
   - New `InvokableCommand` at `Commands/ViewHourlyCommand.cs`
   - Takes location, weather service, settings manager
   - Returns `CommandResult.GoToPage()` to navigate to hourly forecast page

5. **Integration:**
   - Modified `WeatherDetailPage.CreateCurrentWeatherItem()` to use `HourlyForecastPage` as the command
   - Current weather item now navigates to hourly forecast when selected (follows existing pattern from WeatherListPage)

**Key Patterns:**
- **Page Navigation:** Pages can act as commands — passed to `ListItem()` constructor, which automatically handles navigation
- **Time Filtering:** `if (time < DateTime.Now) continue;` filters past hours in hourly forecast
- **Caching Strategy:** 15-minute cache expiration with `latitude,longitude,tempUnit,windUnit` composite key (matches current weather pattern)
- **JSON Context:** All new models registered in source-generated `WeatherJsonContext` for AOT compatibility

**Files Modified (5):**
- `WeatherExtension/Models/ForecastData.cs` — Added HourlyForecastData and HourlyForecast models
- `WeatherExtension/Services/IWeatherService.cs` — Added GetHourlyForecastAsync method
- `WeatherExtension/Services/OpenMeteoService.cs` — Implemented hourly forecast API with caching
- `WeatherExtension/Services/WeatherJsonContext.cs` — Registered HourlyForecastData for JSON serialization
- `WeatherExtension/Pages/WeatherDetailPage.cs` — Wired HourlyForecastPage to current weather item

**Files Created (2):**
- `WeatherExtension/Pages/HourlyForecastPage.cs` — Hourly forecast list page
- `WeatherExtension/Commands/ViewHourlyCommand.cs` — Command to navigate to hourly forecast (created but not directly used)

**Build Status:** ✅ Compiles successfully with 1 pre-existing warning (KillRunningExecutable not found).

## Cross-Agent Updates

📌 Team update (2026-03-02T19:43:19Z): Hourly forecast feature complete — HourlyForecastData models, GetHourlyForecastAsync API with 15-min caching, HourlyForecastPage list page, ViewHourlyCommand, integrated into WeatherDetailPage current weather item — decided by Scarlett

