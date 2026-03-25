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
				var results = await SearchPostalCodeAsync(trimmedQuery, ct).ConfigureAwait(false);
				if (results.Count > 0)
				{
					return results;
				}
			}

			// Progressive fallback: try the full query first, then strip
			// trailing comma-separated segments until results are found.
			// E.g. "Birmingham, Alabama, US" → "Birmingham, Alabama" → "Birmingham"
			var searchResults = await SearchWithProgressiveFallbackAsync(trimmedQuery, ct).ConfigureAwait(false);

			return RankResults(trimmedQuery, searchResults);
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

	private async Task<List<GeocodingResult>> SearchWithProgressiveFallbackAsync(string query, CancellationToken ct)
	{
		var currentQuery = query;

		while (!string.IsNullOrWhiteSpace(currentQuery))
		{
			var results = await SearchNominatimAsync(currentQuery, ct).ConfigureAwait(false);
			if (results.Count > 0)
			{
				return results;
			}

			// Strip the last comma-separated segment and retry
			var lastComma = currentQuery.LastIndexOf(',');
			if (lastComma <= 0)
			{
				break;
			}

			currentQuery = currentQuery[..lastComma].Trim();
		}

		return [];
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
			var url = $"{NominatimUrl}?postalcode={Uri.EscapeDataString(postalCode)}&format=json&addressdetails=1&limit=10";
			var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				return [];
			}

			var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
			var nominatimResults = JsonSerializer.Deserialize<List<NominatimResult>>(content, WeatherJsonContext.Default.ListNominatimResult);

			if (nominatimResults == null)
			{
				ExtensionHost.LogMessage(new LogMessage
				{
					Message = $"Nominatim deserialization returned null. Status: {response.StatusCode}, Content length: {content.Length}",
				});
				return [];
			}

			if (nominatimResults.Count == 0)
			{
				return [];
			}

			return ConvertNominatimResults(nominatimResults);
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

	private async Task<List<GeocodingResult>> SearchNominatimAsync(string query, CancellationToken ct)
	{
		var url = $"{NominatimUrl}?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit=10";
		var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			ExtensionHost.LogMessage(new LogMessage
			{
				Message = $"Nominatim API returned status {response.StatusCode}",
			});
			return [];
		}

		var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
		var nominatimResults = JsonSerializer.Deserialize<List<NominatimResult>>(content, WeatherJsonContext.Default.ListNominatimResult);

		if (nominatimResults == null)
		{
			var contentPreview = content.Length > 200 ? content[..200] : content;
			ExtensionHost.LogMessage(new LogMessage
			{
				Message = $"Nominatim deserialization returned null. Status: {response.StatusCode}, Content length: {content.Length}, Content preview: {contentPreview}",
			});
			return [];
		}

		return ConvertNominatimResults(nominatimResults);
	}

	internal static List<GeocodingResult> ConvertNominatimResults(List<NominatimResult> nominatimResults)
	{
		var results = new List<GeocodingResult>();

		foreach (var nr in nominatimResults)
		{
			var placeName = nr.Name
				?? nr.Address?.City
				?? nr.Address?.Town
				?? nr.Address?.Village
				?? nr.DisplayName?.Split(',')[0].Trim()
				?? string.Empty;

			results.Add(new GeocodingResult
			{
				Latitude = nr.Lat,
				Longitude = nr.Lon,
				Name = placeName,
				Admin1 = nr.Address?.State,
				Country = nr.Address?.Country ?? ExtractCountryFromDisplayName(nr.DisplayName),
				CountryCode = nr.Address?.CountryCode?.ToUpperInvariant(),
			});
		}

		return results;
	}

	internal static List<GeocodingResult> RankResults(string query, List<GeocodingResult> results)
	{
		if (string.IsNullOrWhiteSpace(query) || results.Count <= 1)
		{
			return results;
		}

		// Use just the city part for matching (everything before the first comma)
		var cityPart = query.Contains(',') ? query.Split(',')[0].Trim() : query.Trim();

		return [.. results.OrderByDescending(r => ScoreResult(cityPart, r))];
	}

	private static int ScoreResult(string cityQuery, GeocodingResult result)
	{
		var name = result.Name ?? string.Empty;

		if (string.Equals(name, cityQuery, StringComparison.OrdinalIgnoreCase))
		{
			return 3;
		}

		if (name.StartsWith(cityQuery, StringComparison.OrdinalIgnoreCase))
		{
			return 2;
		}

		if (name.Contains(cityQuery, StringComparison.OrdinalIgnoreCase))
		{
			return 1;
		}

		return 0;
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

	public void Dispose()
	{
		_httpClient?.Dispose();
	}
}
