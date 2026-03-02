// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.Services;

public sealed partial class GeocodingService : IDisposable
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://geocoding-api.open-meteo.com/v1/search";

    public GeocodingService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10),
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PowerToys-CmdPal-Weather/1.0");
    }

    public async Task<List<GeocodingResult>> SearchLocationAsync(string query, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return [];
            }

            // Open-Meteo geocoding works best with just the city name.
            // Try the full query first; if no results, retry with just
            // the first comma-separated token (e.g. "Seattle, WA" → "Seattle").
            var results = await SearchLocationCoreAsync(query, ct).ConfigureAwait(false);
            if (results.Count == 0 && query.Contains(','))
            {
                var cityOnly = query.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(cityOnly))
                {
                    results = await SearchLocationCoreAsync(cityOnly, ct).ConfigureAwait(false);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Geocoding error: {ex.Message}",
            });
            return [];
        }
    }

    private async Task<List<GeocodingResult>> SearchLocationCoreAsync(string query, CancellationToken ct)
    {
        var url = $"{BaseUrl}?name={Uri.EscapeDataString(query)}&count=10&language=en&format=json";
        var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            ExtensionHost.LogMessage(new LogMessage
            {
                Message = $"Geocoding API returned status {response.StatusCode}",
            });
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var wrapper = JsonSerializer.Deserialize<GeocodingResponse>(content, WeatherJsonContext.Default.GeocodingResponse);

        return wrapper?.Results ?? [];
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
