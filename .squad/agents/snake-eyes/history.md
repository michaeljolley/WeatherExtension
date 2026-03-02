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

## Cross-Agent Updates

📌 Team update (2026-03-01T23:45:00Z): Weather extension now in standalone repo with Microsoft.CmdPal.Ext.Weather namespace preserved, ManagedCommon replaced with standard .NET, main project builds successfully — decided by Scarlett


