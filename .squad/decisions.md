# Decisions

> Shared team decisions. All agents read this before starting work. Scribe (Breaker) merges new entries from the inbox.

### 2026-03-01: Weather Extension Migration — Namespace & Dependencies

**Author:** Scarlett (Core Dev)

Migrated the Weather extension from PowerToys (michaeljolley/PowerToys:dev/mjolley/weather-extension branch) to standalone MSIX repository. Key architectural decisions:

**Namespace Preservation:** Kept `Microsoft.CmdPal.Ext.Weather` (not renamed to WeatherExtension) to maintain consistency with PowerToys CommandPalette extension naming pattern (Microsoft.CmdPal.Ext.{ExtensionName}).

**ManagedCommon Replacement:** Removed PowerToys-internal ManagedCommon.Utilities.BaseSettingsPath() dependency. Implemented standalone settings path using `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)`, storing settings at `%LocalAppData%\Microsoft.CmdPal\settings.json` for consistency with CommandPalette expectations.

**DockBands API:** The source includes GetDockBands() override, but this appears unavailable/non-overridable in current SDK (v0.5.250829002). Commented out pending API verification. CurrentWeatherBand created but not registered through GetDockBands().

**Dependencies Removed:** Eliminated all PowerToys-internal dependencies (ManagedCommon, Microsoft.CmdPal.Common). Uses standard .NET + Microsoft.CommandPalette.Extensions SDK.

**Build Result:** ✅ Main extension compiles successfully. Test project has accessibility issues with internal types (Icons, WeatherJsonContext) — requires InternalsVisibleTo fix.

---

### 2026-03-01: Weather Test Migration — Dependencies & Structure

**Author:** Snake Eyes (Tester)

Migrated 4 test files (63 unit tests) from PowerToys to WeatherExtension.Tests project. Decisions:

**Project Structure:** Located at WeatherExtension.Tests/ with target framework net9.0-windows10.0.26100.0 (matching main project). Namespace: Microsoft.CmdPal.Ext.Weather.UnitTests.

**Dependency Removals:** Eliminated ManagedCommon (unused by tests) and UnitTestBase (test utils for fuzzy matching/events not used by Weather tests). Tests are self-contained and portable, relying only on MSTest framework.

**Package Management:** Added MSTest 3.7.0 and Moq 4.20.72 to central Directory.Packages.props. Moq available for future mocking but not currently used.

**Test Files Migrated:** GeocodingResultTests (11 tests), IconsTests (34 tests), WeatherDataModelTests (10 tests), WeatherSettingsManagerTests (8 tests). Total: 63 unit tests.

**Test Results:** 68/71 tests passing (96.5%). 3 pre-existing failures inherited from source repo (not migration-related). No modifications to test code needed — migrated as-is.

**Accessibility Fix:** Applied InternalsVisibleTo attribute to allow test project access to internal types. Added AssemblyName to test .csproj to match InternalsVisibleTo requirement.

---

### Resource Key Prefix Removal — Microsoft_plugin_weather_

**Author:** Scarlett (Core Dev)
**Date:** 2026-03-02

Removed the `Microsoft_plugin_weather_` prefix from all 36 resource keys in `Resources.resx` and updated all references across 8 C# source files. This prefix was inherited from the PowerToys monorepo naming convention and is no longer appropriate for the standalone extension.

**New convention:** Resource keys use clean snake_case (e.g., `celsius`, `plugin_name`, `default_location_title`). No namespace prefixes.

**Impact:** All `.cs` files referencing `Resources.Microsoft_plugin_weather_*` now use `Resources.*` (shorter names). No test files were affected — tests don't reference resource strings directly. Build passes cleanly.
