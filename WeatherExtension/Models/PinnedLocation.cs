// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Microsoft.CmdPal.Ext.Weather.Models;

public sealed class PinnedLocation
{
	[JsonPropertyName("latitude")]
	public double Latitude { get; set; }

	[JsonPropertyName("longitude")]
	public double Longitude { get; set; }

	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("admin1")]
	public string? Admin1 { get; set; }

	[JsonPropertyName("country")]
	public string? Country { get; set; }

	public GeocodingResult ToGeocodingResult()
	{
		return new GeocodingResult
		{
			Latitude = Latitude,
			Longitude = Longitude,
			Name = Name,
			Admin1 = Admin1,
			Country = Country,
		};
	}
}
