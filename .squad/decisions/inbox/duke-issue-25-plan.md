# Issue #25: Hourly Forecasts Stop at the Day — Root Cause Analysis

**Author:** Duke (Lead)
**Date:** 2026-03-20

---

## Summary

Hourly forecasts are truncated at midnight because the API is only asked for 1 day of data. This affects both the command palette hourly list and the dock band's "next 3 hours" display. The fix requires a single API parameter change plus one UI-layer guard.

---

## Layer-by-Layer Analysis

### 1. API Layer — Root Cause

**File:** `WeatherExtension/Services/OpenMeteoService.cs`, line 180

```csharp
var url = $"{BaseUrl}?latitude={latitude}&longitude={longitude}" +
    $"&hourly=temperature_2m,apparent_temperature,weather_code,precipitation_probability,wind_speed_10m,relative_humidity_2m" +
    $"&temperature_unit={temperatureUnit}&wind_speed_unit={windSpeedUnit}&forecast_days=1&timezone=auto";
```

**Problem:** `forecast_days=1` tells Open-Meteo to return hourly data for **only the current calendar day** (00:00–23:00). At 10 PM, that leaves at most 1–2 future hours. At 11 PM, only 1 hour. This is the **primary root cause** of both symptoms.

**Fix:** Change `forecast_days=1` to `forecast_days=2`. This returns 48 hours of data (today + tomorrow), giving every downstream consumer enough hours to work with regardless of time of day.

---

### 2. Data Models — No Changes Needed

**File:** `WeatherExtension/Models/ForecastData.cs`, lines 44–81

The `HourlyForecastData` / `HourlyForecast` classes use `List<string>` for times and `List<double/int>` for values. These are flexible list structures that can hold any number of entries. Changing `forecast_days` from 1 to 2 will return 48 entries instead of 24 — the models handle this without modification.

---

### 3. UI Layer — Command Palette (Hourly Forecast Page)

**File:** `WeatherExtension/Pages/HourlyForecastPage.cs`, lines 88–155

**Current filtering logic (lines 94–114):**
```csharp
var now = DateTime.Now;  // line 94

for (var i = 0; i < count; i++)
{
    // ... null checks ...
    if (time < now)       // line 111 — skip past hours ✓
    {
        continue;         // line 113
    }
    // ... add item to list ...
}
```

**Problem:** The past-hour filter (`time < now`) is correct. But with `forecast_days=1`, there are no hours past midnight to show. Once the API returns 2 days of data, this loop will show all remaining hours through end-of-tomorrow (up to ~48 items), which is more than needed.

**Fix:** Add a 24-hour cap after the past-hour filter. After line 113, add:
```csharp
if (time > now.AddHours(24))
{
    break;
}
```
This ensures the command palette shows exactly the **next 24 hours** of forecasts, not a full 48-hour dump.

---

### 4. UI Layer — Dock Band Card (Next 3 Hours)

**File:** `WeatherExtension/Pages/WeatherBandCard.cs`, lines 159–207

**Current logic (lines 161–206):**
```csharp
var now = DateTime.Now;    // line 161
var hoursAdded = 0;        // line 162

for (var i = 0; i < (hourly.Hourly.Time?.Count ?? 0) && hoursAdded < 3; i++)
{
    // ... parse time, skip if hourTime <= now ...
    // ... fill hour1/hour2/hour3 slots ...
    hoursAdded++;
}
```

**Problem:** This correctly iterates hourly data looking for the next 3 future hours, capping at 3. But when `forecast_days=1` and it's late evening, there are fewer than 3 future hours in the data. Hour slots remain at their `"--"` defaults.

**Fix:** No code changes needed in this file. Once the API returns 2 days of data, this loop will always find 3 future hours to fill the slots — even at 11:59 PM it will pull from tomorrow's hours.

---

### 5. Timezone Handling — Secondary Concern (P2)

**Files:** `HourlyForecastPage.cs` line 94, `WeatherBandCard.cs` line 161

Both use `DateTime.Now` (machine local time) to compare against API times. The API uses `timezone=auto`, which returns times in the **searched location's timezone**. If a user in New York searches for Tokyo weather, `DateTime.Now` is EST but API times are JST — the filter will show wrong hours.

**This is a pre-existing issue unrelated to issue #25.** It's worth a separate ticket (P2) but should not block this fix.

---

## Fix Summary

| Priority | File | Line(s) | Change |
|----------|------|---------|--------|
| **P0** | `Services/OpenMeteoService.cs` | 180 | `forecast_days=1` → `forecast_days=2` |
| **P0** | `Pages/HourlyForecastPage.cs` | After 113 | Add `if (time > now.AddHours(24)) break;` |
| None | `Pages/WeatherBandCard.cs` | — | No changes needed (self-heals with API fix) |
| None | `Models/ForecastData.cs` | — | No changes needed (lists are flexible) |

**Blast Radius:** Very low. One API parameter change (doubles hourly data payload but still tiny — ~48 entries of simple JSON). One guard clause to cap display at 24 hours. No model changes, no breaking changes.

**Testing Requirements:**
- Unit test: verify `CreateHourlyItems` with 48-hour data returns exactly 24 future-hour items
- Unit test: verify `CreateHourlyItems` filters out past hours correctly
- Manual test: at various times of day (especially late evening), verify hourly list shows ~24 hours into the future
- Manual test: dock band expanded view always shows 3 future hours even at 11 PM
