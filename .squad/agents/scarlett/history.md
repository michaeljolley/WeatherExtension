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

## Cross-Agent Updates

📌 Team update (2026-03-01T23:45:00Z): Test project now includes MSTest 3.7.0 + Moq 4.20.72 in central package management, 63 unit tests migrated with 68/71 passing — decided by Snake Eyes


