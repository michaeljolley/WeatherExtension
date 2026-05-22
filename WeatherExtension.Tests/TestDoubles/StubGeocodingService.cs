// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests.TestDoubles;

internal sealed class StubGeocodingService : IGeocodingService
{
	public List<GeocodingResult> Results { get; set; } =
	[
		new GeocodingResult
		{
			Name = "Istanbul",
			Latitude = 41.0082,
			Longitude = 28.9784,
			Admin1 = "Istanbul",
			Country = "Turkey",
		},
	];

	public int SearchCallCount { get; private set; }

	public Task<List<GeocodingResult>> SearchLocationAsync(string query, CancellationToken ct = default)
	{
		SearchCallCount++;
		ct.ThrowIfCancellationRequested();
		return Task.FromResult(Results);
	}

	public void Dispose()
	{
	}
}
