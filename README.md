# Weather Extension for Microsoft Command Palette

A weather extension for [Microsoft Command Palette](https://github.com/microsoft/PowerToys)
(PowerToys) that provides current conditions, hourly forecasts, and multi-day
forecasts right from your desktop.

![Screenshot placeholder — main search view](<!-- TODO: Add screenshot -->)

## Features

- **Current Weather** — Temperature, feels like, humidity, wind speed, and
  conditions for any location worldwide
- **Hourly Forecast** — View the remaining hours of the day with temperature,
  precipitation chance, and conditions
- **3-Day Forecast** — Quick glance at upcoming weather with highs, lows, and
  conditions
- **Dock Bands** — Pin your favorite locations to the Command Palette dock for
  at-a-glance weather cards
- **Postal Code Search** — Search by postal/ZIP code or city name
- **Configurable Units** — Fahrenheit/Celsius, mph/km/h, and adjustable update
  intervals

## Screenshots

### Search & Current Conditions

![Screenshot placeholder — current conditions detail](<!-- TODO: Add screenshot -->)

### Dock Band Card

![Screenshot placeholder — dock band card](<!-- TODO: Add screenshot -->)

### Hourly Forecast

![Screenshot placeholder — hourly forecast](<!-- TODO: Add screenshot -->)

### Settings

![Screenshot placeholder — settings page](<!-- TODO: Add screenshot -->)

## Getting Started

### Prerequisites

- Windows 10/11
- [PowerToys](https://github.com/microsoft/PowerToys) with Command Palette
  enabled
- .NET 9.0 SDK (for building from source)

### Configuration

Open Command Palette and navigate to the Weather extension settings to
configure:

| Setting | Description | Default |
|---------|-------------|---------|
| Default Location | Postal/ZIP code or city name | `98101` |
| Temperature Unit | Fahrenheit or Celsius | Fahrenheit |
| Wind Speed Unit | mph, km/h, m/s, or knots | mph |
| Update Interval | How often weather refreshes | 10 minutes |

## Data Sources

- Weather data provided by [Open-Meteo](https://open-meteo.com/) (free, no API
  key required)
- Geocoding by [Open-Meteo Geocoding API](https://geocoding-api.open-meteo.com/)
  with [Nominatim](https://nominatim.openstreetmap.org/) fallback for postal
  codes

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file
for details.
