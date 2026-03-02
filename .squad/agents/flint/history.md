# Project Context

- **Owner:** Michael Jolley
- **Project:** Weather extension for Microsoft Command Palette (PowerToys)
- **Stack:** C#, .NET, WinUI, Microsoft.CommandPalette.Extensions SDK
- **Created:** 2026-03-01T23:40:31Z

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-01: Adaptive Card Layout Patterns for WeatherBandCard

- **File:** `WeatherExtension/Pages/WeatherBandCard.cs` contains the dock band card UI
- **Pattern:** To center content at ~60% width in Adaptive Cards, use a 3-column wrapper with proportional widths (1:3:1)
- **Spacing:** Changed from "small" to "medium" spacing between icon/temp/condition for better visual breathing room
- **Column alignment:** Added `"horizontalAlignment": "center"` to the column itself to ensure proper centering of the content square
- **Data/template separation:** The card uses a clean separation - `GetCardTemplate()` returns static JSON template, data binding happens via `BuildWeatherData()` method
- **User preference:** Michael prefers the current weather section (temp/icon/condition + facts) to be visually distinct and not span full card width

### 2026-03-02: Dock Band Card Updated with Hourly Forecast

- **Task:** Split current weather section into two 50/50 columns — left: existing current weather, right: next 3 hours forecast
- **Implementation:** Removed the 1:3:1 centering wrapper, replaced with full-width two-column layout (both width="1")
- **Hourly data:** Added call to `GetHourlyForecastAsync` (already existed in `OpenMeteoService`) to fetch next 24 hours with temp, weather code, and precipitation probability
- **Filtering logic:** Only show hours >= current time (filter out past hours). Time displayed in 12-hour format ("h tt" format)
- **Data fields added:** `hour1Time`, `hour1Icon`, `hour1Temp`, `hour1Precip` (and hour2/hour3 variants) to data JSON
- **Card layout:** "Next Hours" section uses ColumnSets for each hour row with time (bolder), icon (medium), temp (bolder), and precip (accent color)
- **Adaptive card best practices:** When splitting content into equal-width columns, use `"width": "1"` for each column (not percentages or fixed widths)

### 2026-03-02: Hourly Forecast Dock Band Implementation

- **Session:** Integrated hourly forecast data into WeatherBandCard dock band
- **Layout:** Split current weather section into 50/50 columns — left: existing current section, right: next 3 hours
- **Hourly Data Fetch:** Added `GetHourlyForecastAsync()` call to retrieve next 24 hours from OpenMeteoService
- **Filtering:** Only display hours >= current time; automatically filters past hours
- **Data Fields:** hour1Time, hour1Icon, hour1Temp, hour1Precip (and variants for hour2/hour3) injected into card data JSON
- **Card Template:** Next Hours section uses ColumnSets with time (bold), icon (medium size), temp (bold), precipitation (accent color)
- **Pattern:** Hourly data binding follows existing patterns from Scarlett's current weather integration
- **Build:** ✅ Compiles successfully with no new warnings
