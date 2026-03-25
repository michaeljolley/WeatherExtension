# Issue #15 Root Cause Analysis & Fix Plan
**Date:** 2026-03-20  
**Analyst:** Duke (Lead/Architect)  
**Issue:** [#15 - Weather in expanded dock window isn't updating](https://github.com/michaeljolley/WeatherExtension/issues/15)

## Problem Statement
The dock band title/subtitle correctly displays current weather (temperature, condition) and updates on a timer. However, when the user clicks the band item to expand it, the content page (WeatherBandCard) shows stale or initial data—it doesn't reflect the current weather/forecast that the band is displaying.

## Root Cause Analysis

### Data Flow Architecture
1. **WeatherCommandsProvider** (provider constructor):
   - Creates a **single shared** `WeatherBandCard` instance (line 32)
   - Creates `CurrentWeatherBand` and passes this shared card (line 33)
   - Band's `Command` property points to the card (CurrentWeatherBand.cs line 32)

2. **CurrentWeatherBand** (timer-based updates):
   - Runs `UpdateWeatherAsync()` on a timer (default 30 minutes)
   - Fetches current weather and forecast from API
   - **Updates its own Title/Subtitle** with latest data (lines 77-98)
   - **Does NOT update the content page** (`_contentPage`)

3. **WeatherBandCard** (expanded view):
   - Calls `LoadWeatherDataAsync()` **ONCE** in constructor (line 44)
   - Fetches weather/forecast and stores in `_weatherForm.DataJson` (line 94)
   - Only refreshes on settings changes (OnSettingsChanged event)
   - **No mechanism to refresh when band timer updates**

### The Bug
**Timing mismatch between band and card:**
- Band fetches fresh data every N minutes → updates Title/Subtitle
- Card fetches data once at startup → never syncs with band's timer
- User sees current weather in band (e.g., "72°F Sunny") but clicks to expand and sees old data from startup (e.g., "68°F Cloudy")

### Why This Happens
The SDK has **no lifecycle methods** (OnNavigatedTo, OnActivated, etc.) to notify ContentPage when it's displayed. The card is instantiated once and reused across all expansions. There's no event or callback when the user expands the dock band.

## Proposed Fix

### Option 1: Sync Card with Band Timer (Recommended)
**Approach:** Make the band refresh the content page whenever it updates itself.

**Implementation:**
```csharp
// In CurrentWeatherBand.UpdateWeatherAsync() (after line 98):
await _contentPage.LoadWeatherDataAsync();
```

**Pros:**
- Minimal code change (one line)
- Ensures card always matches band data
- Works for both CurrentWeatherBand and PinnedWeatherBand

**Cons:**
- Card fetches data even when not visible (minor overhead)
- Two API calls per update cycle (band + card both call geocoding/weather APIs)

### Option 2: Pass Weather Data from Band to Card
**Approach:** Band fetches data and passes it to card via a new method like `UpdateWeatherData(location, weather, forecast)`.

**Pros:**
- Single API call per update
- More efficient (no duplicate fetches)

**Cons:**
- Requires new API surface on WeatherBandCard
- More code changes (3 files)
- Tighter coupling between band and card

### Option 3: Make Card Internal Method Public
**Approach:** Change `LoadWeatherDataAsync()` from internal to public, document it as refresh API.

**Pros:**
- Semantic clarity ("this is the refresh method")
- Opens future extensibility

**Cons:**
- Breaking visibility change
- Still makes duplicate API calls

## Recommended Solution

**Fix:** Option 1 (Sync Card with Band Timer)

**Changes:**
1. **CurrentWeatherBand.cs** (line 99, after weather update):
   ```csharp
   await _contentPage.LoadWeatherDataAsync();
   ```

2. **PinnedWeatherBand.cs** (line 87, after weather update):
   ```csharp
   await _contentPage.LoadWeatherDataAsync();
   ```

**Files affected:** 2 (both DockBand files)

## Risk Assessment

**Blast Radius:** Low
- Two small, focused changes
- No API surface changes
- No schema changes

**Performance Impact:** Low
- Extra API calls only when timer fires (default 30min)
- Async execution doesn't block UI
- Card already has proper error handling

**Regression Risk:** Low
- Adds behavior, doesn't change existing flow
- Card's `LoadWeatherDataAsync()` is already battle-tested (called on settings changes)
- No changes to data models or serialization

**Edge Cases:**
- If card load fails, band still shows data (already handled—card has independent error handling)
- Rapid settings changes could queue multiple refreshes (already handled—async with CancellationToken)

## Testing Recommendations

### Manual Testing
1. Start extension, verify band shows weather
2. Wait for timer to fire (or trigger settings change to force update)
3. Expand dock band **before and after** timer update
4. Verify expanded card shows **same data** as band title/subtitle

### Unit Testing (if time permits)
1. Mock timer in CurrentWeatherBand, verify `LoadWeatherDataAsync()` called on `_contentPage`
2. Test error case: band updates but card load fails (verify band still shows data)

### Regression Testing
1. Verify pinned location bands work correctly
2. Verify settings changes still trigger card refresh
3. Verify band timer interval respects settings

## Implementation Notes

**Async/Await Caution:**
- `UpdateWeatherAsync()` is already async
- Adding `await _contentPage.LoadWeatherDataAsync()` is safe (exception handling already in place)
- Card's CancellationToken prevents orphaned requests on disposal

**Error Isolation:**
- Card load failure doesn't affect band (separate try/catch blocks)
- Band shows last known good data even if card fails

**Future Improvements (Out of Scope):**
- Option 2 (pass data from band to card) could be pursued in a future PR to reduce API calls
- Add telemetry to track card refresh frequency vs. user expansions

## Decision

**Status:** Awaiting approval  
**Proposed by:** Duke  
**Reviewers:** Michael Jolley (Product Owner), Scarlett (implementer if approved)
