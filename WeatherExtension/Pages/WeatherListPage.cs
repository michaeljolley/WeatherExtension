// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.CmdPal.Ext.Weather.Commands;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.Pages;

internal sealed partial class WeatherListPage : DynamicListPage, IDisposable
{
    private static readonly CompositeFormat NoResultsFormat = CompositeFormat.Parse(Resources.no_results_for);

    private readonly IWeatherService _weatherService;
    private readonly GeocodingService _geocodingService;
    private readonly WeatherSettingsManager _settingsManager;
    private readonly Lock _sync = new();
    private readonly CancellationTokenSource _cts = new();

    private IListItem[] _items = [];
    private string _lastSearchQuery = string.Empty;
    private bool _isSearching;

    public WeatherListPage(
        IWeatherService weatherService,
        GeocodingService geocodingService,
        WeatherSettingsManager settingsManager)
    {
        ArgumentNullException.ThrowIfNull(weatherService);
        ArgumentNullException.ThrowIfNull(geocodingService);
        ArgumentNullException.ThrowIfNull(settingsManager);

        _weatherService = weatherService;
        _geocodingService = geocodingService;
        _settingsManager = settingsManager;

        Name = "Weather";
        Title = "Weather";
        Icon = Icons.WeatherIcon;
        Id = "com.baldbeardedbuilder.cmdpal.weather.list";
        PlaceholderText = Resources.search_placeholder;
        ShowDetails = true;

        LoadDefaultLocationWeather();

        _settingsManager.Settings.SettingsChanged += OnSettingsChanged;
    }

    private async void LoadDefaultLocationWeather()
    {
        try
        {
            var defaultLocation = _settingsManager.DefaultLocation;
            if (string.IsNullOrWhiteSpace(defaultLocation))
            {
                return;
            }

            var locations = await _geocodingService.SearchLocationAsync(defaultLocation, _cts.Token).ConfigureAwait(false);
            if (locations.Count > 0)
            {
                await LoadWeatherForLocation(locations[0]).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Failed to load default location weather: {ex.Message}",
            });
        }
    }

    private async Task LoadWeatherForLocation(GeocodingResult location)
    {
        try
        {
            var weatherData = await _weatherService.GetCurrentWeatherAsync(
                location.Latitude,
                location.Longitude,
                _settingsManager.TemperatureUnit,
                _settingsManager.WindSpeedUnit,
                _cts.Token).ConfigureAwait(false);

            if (weatherData?.Current == null)
            {
                return;
            }

            var items = new List<IListItem>
            {
                CreateWeatherItem(location, weatherData),
            };

            lock (_sync)
            {
                _items = items.ToArray();
            }

            RaiseItemsChanged();
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Failed to load weather for location: {ex.Message}",
            });
        }
    }

    private ListItem CreateWeatherItem(GeocodingResult location, WeatherData weatherData)
    {
        if (weatherData?.Current == null)
        {
            return new ListItem(new NoOpCommand())
            {
                Title = location.DisplayName,
                Subtitle = Resources.no_data_available,
                Icon = Icons.WeatherIcon,
            };
        }

        var current = weatherData.Current;
        var tempUnit = _settingsManager.TemperatureUnit == "celsius" ? "°C" : "°F";
        var windUnit = _settingsManager.WindSpeedUnit == "mph" ? "mph" : "km/h";
        var condition = Icons.GetWeatherDescription(current.WeatherCode);

        var detailPage = new WeatherDetailPage(
            location,
            _weatherService,
            _settingsManager);

        var item = new ListItem(detailPage)
        {
            Title = location.DisplayName,
            Subtitle = $"{condition} — {current.Temperature:F0}{tempUnit} (feels like {current.ApparentTemperature:F0}{tempUnit})",
            Icon = Icons.GetIconForWeatherCode(current.WeatherCode),
            Details = new Details
            {
                Title = location.DisplayName,
                Body = $"{condition} — {current.Temperature:F0}{tempUnit} (feels like {current.ApparentTemperature:F0}{tempUnit})",
                Metadata =
                [
                    new DetailsElement { Key = "Humidity", Data = new DetailsLink($"{current.RelativeHumidity}%") },
                    new DetailsElement { Key = "Wind", Data = new DetailsLink($"{current.WindSpeed:F1} {windUnit}") },
                    new DetailsElement { Key = "Wind Direction", Data = new DetailsLink(GetWindDirection(current.WindDirection)) },
                ],
            },
            MoreCommands =
            [
                new CommandContextItem(new RefreshWeatherCommand(this)),
            ],
        };

        return item;
    }

    private static string GetWindDirection(int degrees)
    {
        var directions = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        var index = (int)Math.Round(degrees / 45.0) % 8;
        return directions[index];
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (_isSearching)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(newSearch))
        {
            LoadDefaultLocationWeather();
            return;
        }

        if (newSearch == _lastSearchQuery)
        {
            return;
        }

        _lastSearchQuery = newSearch;
        _ = PerformSearchAsync(newSearch);
    }

    private async Task PerformSearchAsync(string query)
    {
        _isSearching = true;

        try
        {
            var locations = await _geocodingService.SearchLocationAsync(query, _cts.Token).ConfigureAwait(false);

            if (locations.Count == 0)
            {
                var noResultsItem = new ListItem(new NoOpCommand())
                {
                    Title = Resources.no_locations_found,
                    Subtitle = string.Format(CultureInfo.CurrentCulture, NoResultsFormat, query),
                    Icon = Icons.WeatherIcon,
                };

                lock (_sync)
                {
                    _items = [noResultsItem];
                }

                RaiseItemsChanged();
                return;
            }

            if (locations.Count == 1)
            {
                await LoadWeatherForLocation(locations[0]).ConfigureAwait(false);
                return;
            }

            var items = new List<IListItem>();
            foreach (var location in locations.Take(10))
            {
                var weatherData = await _weatherService.GetCurrentWeatherAsync(
                    location.Latitude,
                    location.Longitude,
                    _settingsManager.TemperatureUnit,
                    _settingsManager.WindSpeedUnit,
                    _cts.Token).ConfigureAwait(false);

                if (weatherData != null)
                {
                    items.Add(CreateWeatherItem(location, weatherData));
                }
            }

            lock (_sync)
            {
                _items = items.ToArray();
            }

            RaiseItemsChanged();
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Search error: {ex.Message}",
            });
        }
        finally
        {
            _isSearching = false;
        }
    }

    private void OnSettingsChanged(object sender, Settings args)
    {
        RefreshWeather();
    }

    public void RefreshWeather()
    {
        if (!string.IsNullOrWhiteSpace(_lastSearchQuery))
        {
            _ = PerformSearchAsync(_lastSearchQuery);
        }
        else
        {
            LoadDefaultLocationWeather();
        }
    }

    public override IListItem[] GetItems()
    {
        lock (_sync)
        {
            return _items;
        }
    }

    public void Dispose()
    {
        _settingsManager.Settings.SettingsChanged -= OnSettingsChanged;
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
