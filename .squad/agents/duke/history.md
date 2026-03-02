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
