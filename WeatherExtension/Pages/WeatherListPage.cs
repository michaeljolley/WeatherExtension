// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
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
    private const int MinSearchLength = 3;

    private readonly IWeatherService _weatherService;
    private readonly GeocodingService _geocodingService;
    private readonly WeatherSettingsManager _settingsManager;
    private readonly PinnedLocationsManager _pinnedLocationsManager;
    private readonly Lock _sync = new();
    private readonly CancellationTokenSource _cts = new();

    private IListItem[] _items = [];
    private bool _isLoading;
    private string _lastSearchQuery = string.Empty;
    private CancellationTokenSource _searchCts = new();

    public WeatherListPage(
        IWeatherService weatherService,
        GeocodingService geocodingService,
        WeatherSettingsManager settingsManager,
        PinnedLocationsManager pinnedLocationsManager)
    {
        ArgumentNullException.ThrowIfNull(weatherService);
        ArgumentNullException.ThrowIfNull(geocodingService);
        ArgumentNullException.ThrowIfNull(settingsManager);
        ArgumentNullException.ThrowIfNull(pinnedLocationsManager);

        _weatherService = weatherService;
        _geocodingService = geocodingService;
        _settingsManager = settingsManager;
        _pinnedLocationsManager = pinnedLocationsManager;

        Name = "Weather";
        Title = "Weather";
        Icon = Icons.WeatherIcon;
        Id = "com.baldbeardedbuilder.cmdpal.weather.list";
        PlaceholderText = Resources.search_placeholder;
        ShowDetails = true;

        LoadDefaultLocationWeather();

        _settingsManager.Settings.SettingsChanged += OnSettingsChanged;
    }

    private async void LoadDefaultLocationWeather(CancellationToken searchCt = default)
    {
        try
        {
            var defaultLocation = _settingsManager.DefaultLocation;
            if (string.IsNullOrWhiteSpace(defaultLocation))
            {
                return;
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(searchCt, _cts.Token);
            var locations = await _geocodingService.SearchLocationAsync(defaultLocation, linkedCts.Token).ConfigureAwait(false);
            if (locations.Count > 0)
            {
                await LoadWeatherForLocation(locations[0], linkedCts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelled by a newer search or page is disposed
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Failed to load default location weather: {ex.Message}",
            });
        }
    }

    private async Task LoadWeatherForLocation(GeocodingResult location, CancellationToken ct)
    {
        try
        {
            var weatherData = await _weatherService.GetCurrentWeatherAsync(
                location.Latitude,
                location.Longitude,
                _settingsManager.TemperatureUnit,
                _settingsManager.WindSpeedUnit,
                ct).ConfigureAwait(false);

            if (weatherData?.Current == null)
            {
                lock (_sync)
                {
                    _isLoading = false;
                }

                RaiseItemsChanged();
                return;
            }

            var items = new List<IListItem>
            {
                CreateWeatherItem(location, weatherData),
            };

            lock (_sync)
            {
                _items = items.ToArray();
                _isLoading = false;
            }

            RaiseItemsChanged();
        }
        catch (Exception ex)
        {
            lock (_sync)
            {
                _isLoading = false;
            }

            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Failed to load weather for location: {ex.Message}",
            });

            RaiseItemsChanged();
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

        var moreCommands = new List<ICommandContextItem>
        {
            new CommandContextItem(new RefreshWeatherCommand(this)),
        };

        if (_pinnedLocationsManager.IsPinned(location))
        {
            moreCommands.Add(new CommandContextItem(new UnpinFromDockCommand(location, _pinnedLocationsManager)));
        }
        else
        {
            moreCommands.Add(new CommandContextItem(new PinToDockCommand(location, _pinnedLocationsManager)));
        }

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
            MoreCommands = moreCommands.ToArray(),
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
        if (string.IsNullOrWhiteSpace(newSearch))
        {
            var ct = ResetSearchToken();
            _lastSearchQuery = string.Empty;
            LoadDefaultLocationWeather(ct);
            return;
        }

        if (newSearch.Trim().Length < MinSearchLength)
        {
            ResetSearchToken();
            _lastSearchQuery = newSearch;
            var minCharsItem = new ListItem(new NoOpCommand())
            {
                Title = Resources.search_min_chars,
                Icon = Icons.WeatherIcon,
            };
            lock (_sync)
            {
                _items = [minCharsItem];
                _isLoading = false;
            }

            RaiseItemsChanged();
            return;
        }

        if (newSearch == _lastSearchQuery)
        {
            return;
        }

        _lastSearchQuery = newSearch;
        var searchCt = ResetSearchToken();
        _ = PerformDebouncedSearchAsync(newSearch, searchCt);
    }

    private CancellationToken ResetSearchToken()
    {
        var oldCts = _searchCts;
        _searchCts = new CancellationTokenSource();
        // Cancel but do not dispose immediately: the fire-and-forget debounced task may still be
        // executing callbacks on oldCts.Token, and disposing it while those callbacks are in flight
        // would throw ObjectDisposedException.  The GC will reclaim it once no references remain.
        oldCts.Cancel();
        return _searchCts.Token;
    }

    private async Task PerformDebouncedSearchAsync(string query, CancellationToken searchCt)
    {
        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(searchCt, _cts.Token);
            await Task.Delay(300, linkedCts.Token).ConfigureAwait(false);
            await PerformSearchAsync(query, linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected when the debounce is cancelled by a newer search or page is disposed
        }
    }

    private async Task PerformSearchAsync(string query, CancellationToken ct)
    {
        try
        {
            lock (_sync)
            {
                _isLoading = true;
            }

            RaiseItemsChanged();

            var locations = await _geocodingService.SearchLocationAsync(query, ct).ConfigureAwait(false);

            // Re-check cancellation after the async call returns so that stale results
            // from an earlier query are never shown when the user has already typed more.
            if (ct.IsCancellationRequested)
            {
                lock (_sync)
                {
                    _isLoading = false;
                }

                RaiseItemsChanged();
                return;
            }

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
                    _isLoading = false;
                }

                RaiseItemsChanged();
                return;
            }

            if (locations.Count == 1)
            {
                await LoadWeatherForLocation(locations[0], ct).ConfigureAwait(false);
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
                    ct).ConfigureAwait(false);

                if (weatherData != null)
                {
                    items.Add(CreateWeatherItem(location, weatherData));
                }
            }

            // Final cancellation check before committing results to the UI.
            if (ct.IsCancellationRequested)
            {
                lock (_sync)
                {
                    _isLoading = false;
                }

                RaiseItemsChanged();
                return;
            }

            lock (_sync)
            {
                _items = items.ToArray();
                _isLoading = false;
            }

            RaiseItemsChanged();
        }
        catch (OperationCanceledException)
        {
            // Expected when the search is cancelled by a newer query or page is disposed
            lock (_sync)
            {
                _isLoading = false;
            }

            RaiseItemsChanged();
        }
        catch (HttpRequestException ex)
        {
            lock (_sync)
            {
                _isLoading = false;
                _items = [new ListItem(new NoOpCommand())
                {
                    Title = Resources.network_error,
                    Icon = Icons.WeatherIcon,
                }];
            }

            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Search network error: {ex.Message}",
            });

            RaiseItemsChanged();
        }
        catch (Exception ex)
        {
            lock (_sync)
            {
                _isLoading = false;
            }

            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Search error: {ex.Message}",
            });

            RaiseItemsChanged();
        }
    }

    private void OnSettingsChanged(object sender, Settings args)
    {
        RefreshWeather();
    }

    public void RefreshWeather()
    {
        var ct = ResetSearchToken();
        if (!string.IsNullOrWhiteSpace(_lastSearchQuery))
        {
            _ = PerformDebouncedSearchAsync(_lastSearchQuery, ct);
        }
        else
        {
            LoadDefaultLocationWeather(ct);
        }
    }

    public override IListItem[] GetItems()
    {
        lock (_sync)
        {
            if (_isLoading)
            {
                return
                [
                    new ListItem(new NoOpCommand())
                    {
                        Title = Resources.loading_data,
                        Icon = Icons.WeatherIcon,
                    },
                ];
            }

            return _items;
        }
    }

    public void Dispose()
    {
        _settingsManager.Settings.SettingsChanged -= OnSettingsChanged;
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _cts?.Cancel();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
