// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Microsoft.CmdPal.Ext.Weather.Models;

public sealed class NominatimResult
{
	[JsonPropertyName("lat")]
	public double Lat { get; set; }

	[JsonPropertyName("lon")]
	public double Lon { get; set; }

	[JsonPropertyName("display_name")]
	public string? DisplayName { get; set; }
}
