# Weather Extension for Microsoft Command Palette

[<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>](https://get.microsoft.com/installer/download/9N01D6BS9V98?referrer=appbadge) ![WinGet Package Version](https://img.shields.io/winget/v/BaldBeardedBuilder.WeatherforCommandPalette)


A weather extension for [Microsoft Command Palette](https://github.com/microsoft/PowerToys)
(PowerToys) that provides current conditions, hourly forecasts, and multi-day
forecasts right from your desktop.

<img width="1027" height="627" alt="image" src="https://github.com/user-attachments/assets/f91171da-e491-46aa-b520-4c5112042d62" />

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

<img width="1023" height="624" alt="image" src="https://github.com/user-attachments/assets/4cbdfb54-a081-41b3-b4ee-1da1274f09da" />

### Dock Band Card

<img width="1112" height="669" alt="image" src="https://github.com/user-attachments/assets/57d4aba6-f788-4a79-b730-82ece6be2148" />

### Hourly Forecast

<img width="1023" height="622" alt="image" src="https://github.com/user-attachments/assets/d87e8810-bc70-4ad4-b3ed-feea30b03e9f" />

### Settings

<img width="1027" height="673" alt="image" src="https://github.com/user-attachments/assets/01962a91-ccdf-42e3-aa28-19e3bcf493a3" />

## Installation

You can install Weather for Command Palette via:

- [Microsoft Store](https://apps.microsoft.com/detail/9N01D6BS9V98)
- Winget via `winget install BaldBeardedBuilder.WeatherforCommandPalette`
- [GitHub Releases](https://github.com/michaeljolley/WeatherExtension/releases)

---

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
