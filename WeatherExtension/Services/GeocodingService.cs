// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Microsoft.CmdPal.Ext.Weather.Services;

public sealed partial class GeocodingService : IDisposable
{
	private readonly HttpClient _httpClient;
	private const string BaseUrl = "https://geocoding-api.open-meteo.com/v1/search";
	private const string NominatimUrl = "https://nominatim.openstreetmap.org/search";
	private const int MinSearchLength = 3;

	[GeneratedRegex(@"^\d{5}(-\d{4})?$")]
	private static partial Regex UsZipCodeRegex();

	[GeneratedRegex(@"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$", RegexOptions.IgnoreCase)]
	private static partial Regex CanadaPostalCodeRegex();

	[GeneratedRegex(@"^\d{4,6}$")]
	private static partial Regex InternationalPostalCodeRegex();

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

			var trimmedQuery = query.Trim();

			if (trimmedQuery.Length < MinSearchLength)
			{
				return [];
			}

			// Check if input looks like a postal code
			if (IsPostalCode(trimmedQuery))
			{
				// Try Open-Meteo first (it can sometimes resolve postal codes)
				var results = await SearchLocationCoreAsync(trimmedQuery, ct).ConfigureAwait(false);
				if (results.Count > 0)
				{
					return results;
				}

				// Fall back to Nominatim for postal code lookup
				results = await SearchPostalCodeAsync(trimmedQuery, ct).ConfigureAwait(false);
				if (results.Count > 0)
				{
					return results;
				}
			}

			// Open-Meteo geocoding works best with just the city name.
			// Try the full query first; if no results, retry with just
			// the first comma-separated token (e.g. "Seattle, WA" → "Seattle").
			var cityResults = await SearchLocationCoreAsync(trimmedQuery, ct).ConfigureAwait(false);
			if (cityResults.Count == 0 && trimmedQuery.Contains(','))
			{
				var cityOnly = trimmedQuery.Split(',')[0].Trim();
				if (!string.IsNullOrWhiteSpace(cityOnly))
				{
					cityResults = await SearchLocationCoreAsync(cityOnly, ct).ConfigureAwait(false);
				}
			}

			return cityResults;
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

	private static bool IsPostalCode(string input)
	{
		return UsZipCodeRegex().IsMatch(input) ||
			   CanadaPostalCodeRegex().IsMatch(input) ||
			   InternationalPostalCodeRegex().IsMatch(input);
	}

	private async Task<List<GeocodingResult>> SearchPostalCodeAsync(string postalCode, CancellationToken ct)
	{
		try
		{
			var url = $"{NominatimUrl}?postalcode={Uri.EscapeDataString(postalCode)}&format=json&limit=1";
			var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				return [];
			}

			var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			var nominatimResults = JsonSerializer.Deserialize<List<NominatimResult>>(content, WeatherJsonContext.Default.ListNominatimResult);

			if (nominatimResults == null || nominatimResults.Count == 0)
			{
				return [];
			}

			// Convert Nominatim results to GeocodingResult
			var results = new List<GeocodingResult>();
			foreach (var nr in nominatimResults)
			{
				results.Add(new GeocodingResult
				{
					Latitude = nr.Lat,
					Longitude = nr.Lon,
					Name = nr.DisplayName?.Split(',')[0].Trim() ?? postalCode,
					Country = ExtractCountryFromDisplayName(nr.DisplayName),
					CountryCode = nr.DisplayName?.Split(',').LastOrDefault()?.Trim(),
				});
			}

			return results;
		}
		catch (Exception ex)
		{
			ExtensionHost.LogMessage(new LogMessage
			{
				Message = $"Nominatim postal code lookup error: {ex.Message}",
			});
			return [];
		}
	}

	private static string ExtractCountryFromDisplayName(string? displayName)
	{
		if (string.IsNullOrWhiteSpace(displayName))
		{
			return string.Empty;
		}

		var parts = displayName.Split(',');
		return parts.Length > 0 ? parts[^1].Trim() : string.Empty;
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
