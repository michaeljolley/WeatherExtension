# WeatherBandCard Hourly Forecast UI Update

**Author:** Flint (UI Dev)
**Date:** 2026-03-02
**Status:** ✅ Implemented

## Summary

Updated the `WeatherBandCard` dock band adaptive card to split the current weather section into two equal columns: left side showing existing current weather (icon, temp, condition, and fact set), right side showing the next 3 hours forecast with time, icon, temperature, and precipitation chance.

## Changes Made

### Layout Changes
- **Removed** the 1:3:1 centering wrapper (previously used to center content at ~60% width)
- **Added** full-width two-column layout with equal widths (`"width": "1"` for both columns)
- **Left column:** Existing current weather display (icon/temp/condition square + FactSet) with emphasis style
- **Right column:** "Next Hours" section showing 3 hourly forecasts, each row displaying time (bold), weather icon, temperature (bold), and precipitation % (accent color)

### Data Changes
- **Added hourly forecast fetch:** `LoadWeatherDataAsync()` now calls `_weatherService.GetHourlyForecastAsync()` (method already existed, implemented by Scarlett)
- **BuildWeatherData() signature:** Added `HourlyForecastData?` parameter
- **New data fields:** `hour1Time`, `hour1Icon`, `hour1Temp`, `hour1Precip` (and hour2/hour3 variants)
- **Filtering logic:** Only show hours >= current UTC time to avoid showing past hours
- **Time format:** 12-hour format with AM/PM ("h tt") using CurrentCulture

### Updated Methods
1. `LoadWeatherDataAsync()` — Added call to `GetHourlyForecastAsync`
2. `BuildWeatherData()` — Added hourly parameter and processing logic
3. `GetLoadingData()` — Added placeholder hourly fields with "--" values
4. `GetErrorData()` — Added placeholder hourly fields with "--" values
5. `GetCardTemplate()` — Restructured adaptive card JSON with new two-column layout

## Technical Decisions

| Decision | Rationale |
|----------|-----------|
| Equal-width columns (`"width": "1"` for both) | Ensures consistent 50/50 split across all card widths |
| Filter hours >= current time | Only show future hours, not past hours that are already over |
| 12-hour time format with AM/PM | More user-friendly than 24-hour format for general audience |
| Precipitation % with accent color | Visually distinguishes rain chance from temperature data |
| Reuse existing `GetHourlyForecastAsync` | Scarlett already implemented the API method — no duplication needed |

## Files Modified

- `WeatherExtension/Pages/WeatherBandCard.cs` — All changes contained in this single file

## Build Status

✅ Compiles successfully. Pre-existing build errors in `ViewHourlyCommand.cs` and `WeatherDetailPage.cs` are unrelated to this change.

## Testing Notes

**Manual testing required:**
1. Deploy to Command Palette and check dock band display
2. Verify current weather section shows on left (icon, temp, condition, facts)
3. Verify next 3 hours section shows on right with correct times (future hours only)
4. Check precipitation % displays correctly
5. Verify layout is responsive and columns are equal width
6. Test with different locales to ensure time formatting works (12-hour format)
