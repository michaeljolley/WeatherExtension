### Resource Key Prefix Removal — Microsoft_plugin_weather_

**Author:** Scarlett (Core Dev)
**Date:** 2026-03-02

Removed the `Microsoft_plugin_weather_` prefix from all 36 resource keys in `Resources.resx` and updated all references across 8 C# source files. This prefix was inherited from the PowerToys monorepo naming convention and is no longer appropriate for the standalone extension.

**New convention:** Resource keys use clean snake_case (e.g., `celsius`, `plugin_name`, `default_location_title`). No namespace prefixes.

**Impact:** All `.cs` files referencing `Resources.Microsoft_plugin_weather_*` now use `Resources.*` (shorter names). No test files were affected — tests don't reference resource strings directly. Build passes cleanly.
