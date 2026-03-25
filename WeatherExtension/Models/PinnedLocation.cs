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
		// Shift lat/lon to non-negative ranges before encoding to avoid negative IDs.
		// latFixed: 0..180,000,000 (lat+90 shifted to non-negative, 6 decimal places)
		// lonFixed: 0..360,000,000 (lon+180 shifted to non-negative, 6 decimal places)
		// Multiplier 400_000_000 > max lonFixed, guaranteeing collision-free IDs.
		var latFixed = (long)Math.Round((Latitude + 90.0) * 1_000_000);
		var lonFixed = (long)Math.Round((Longitude + 180.0) * 1_000_000);
		return new GeocodingResult
		{
			Id = (latFixed * 400_000_000L) + lonFixed,
			Latitude = Latitude,
			Longitude = Longitude,
			Name = Name,
			Admin1 = Admin1,
			Country = Country,
		};
	}
}
