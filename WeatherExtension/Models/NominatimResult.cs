// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Microsoft.CmdPal.Ext.Weather.Models;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public sealed class NominatimResult
{
	[JsonPropertyName("place_id")]
	public long PlaceId { get; set; }

	[JsonPropertyName("lat")]
	public double Lat { get; set; }

	[JsonPropertyName("lon")]
	public double Lon { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("display_name")]
	public string? DisplayName { get; set; }

	[JsonPropertyName("address")]
	public NominatimAddress? Address { get; set; }
}

public sealed class NominatimAddress
{
	[JsonPropertyName("city")]
	public string? City { get; set; }

	[JsonPropertyName("town")]
	public string? Town { get; set; }

	[JsonPropertyName("village")]
	public string? Village { get; set; }

	[JsonPropertyName("state")]
	public string? State { get; set; }

	[JsonPropertyName("country")]
	public string? Country { get; set; }

	[JsonPropertyName("country_code")]
	public string? CountryCode { get; set; }
}
