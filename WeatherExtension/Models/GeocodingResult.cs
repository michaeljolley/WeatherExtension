// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Microsoft.CmdPal.Ext.Weather.Models;

public sealed class GeocodingResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("admin1")]
    public string? Admin1 { get; set; }

    [JsonPropertyName("admin2")]
    public string? Admin2 { get; set; }

    [JsonPropertyName("admin3")]
    public string? Admin3 { get; set; }

    [JsonPropertyName("admin4")]
    public string? Admin4 { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    public string DisplayName
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                parts.Add(Name);
            }

            if (!string.IsNullOrWhiteSpace(Admin1))
            {
                parts.Add(Admin1);
            }

            if (!string.IsNullOrWhiteSpace(Country))
            {
                parts.Add(Country);
            }

            return string.Join(", ", parts);
        }
    }
}
