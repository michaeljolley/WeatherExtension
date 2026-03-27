// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class GeocodingServiceTests
{
	[DataTestMethod]
	[DataRow("90210")] // Valid US ZIP code
	[DataRow("K1A 0B1")] // Valid Canadian postal code
	[DataRow("k1a 0b1")] // Valid Canadian postal code in lowercase
	[DataRow("SW1A 1AA")] // Valid UK postal codes
	[DataRow("SW1A1AA")] // Valid UK postal codes without space
	[DataRow("sw1a 1aa")] // Valid UK postal codes in lowercase
	[DataRow("M1 1AJ")] // Valid UK postal codes
	public async Task SearchLocationAsync_WithUkPostcode_UsesPostalCodeQueryParameter(string input)
	{
		var seenUris = new List<Uri>();
		var handler = new CountingHttpHandler(_ =>
		new HttpResponseMessage(HttpStatusCode.OK),
		request => seenUris.Add(request.RequestUri!));

		using var service = new GeocodingService(handler);
		var results = await service.SearchLocationAsync(input, CancellationToken.None);

		var decodedQuery = Uri.UnescapeDataString(seenUris[0].Query);
		Assert.IsTrue(decodedQuery.Contains($"postalcode={input}", StringComparison.Ordinal), "Expected postcode input to be sent via postalcode parameter.");
	}

	[TestMethod]
	public void RankResults_WithEmptyQuery_ReturnsOriginalOrder()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "Portland" },
			new() { Name = "Seattle" },
		};

		var ranked = GeocodingService.RankResults(string.Empty, results);

		Assert.AreEqual("Portland", ranked[0].Name);
		Assert.AreEqual("Seattle", ranked[1].Name);
	}

	[TestMethod]
	public void RankResults_WithSingleResult_ReturnsOriginalList()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "Seattle" },
		};

		var ranked = GeocodingService.RankResults("Seattle", results);

		Assert.AreEqual(1, ranked.Count);
		Assert.AreEqual("Seattle", ranked[0].Name);
	}

	[TestMethod]
	public void RankResults_ExactMatchRankedFirst()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "New Seattle" },
			new() { Name = "Seattle Heights" },
			new() { Name = "Seattle" },
		};

		var ranked = GeocodingService.RankResults("Seattle", results);

		Assert.AreEqual("Seattle", ranked[0].Name);
	}

	[TestMethod]
	public void RankResults_StartsWithRankedBeforeContains()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "East Seattle" },
			new() { Name = "Seattle Heights" },
		};

		var ranked = GeocodingService.RankResults("Seattle", results);

		Assert.AreEqual("Seattle Heights", ranked[0].Name);
		Assert.AreEqual("East Seattle", ranked[1].Name);
	}

	[TestMethod]
	public void RankResults_IsCaseInsensitive()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "Seattle Heights" },
			new() { Name = "SEATTLE" },
		};

		var ranked = GeocodingService.RankResults("seattle", results);

		Assert.AreEqual("SEATTLE", ranked[0].Name);
		Assert.AreEqual("Seattle Heights", ranked[1].Name);
	}

	[TestMethod]
	public void RankResults_WithCommaInQuery_UsesOnlyCityPart()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "Portland" },
			new() { Name = "Seattle" },
		};

		var ranked = GeocodingService.RankResults("Seattle, WA", results);

		Assert.AreEqual("Seattle", ranked[0].Name);
		Assert.AreEqual("Portland", ranked[1].Name);
	}

	[TestMethod]
	public void RankResults_NoMatchResultsRetainOriginalRelativeOrder()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "Portland" },
			new() { Name = "Denver" },
			new() { Name = "Houston" },
		};

		var ranked = GeocodingService.RankResults("Seattle", results);

		Assert.AreEqual(3, ranked.Count);
		Assert.AreEqual("Portland", ranked[0].Name);
		Assert.AreEqual("Denver", ranked[1].Name);
		Assert.AreEqual("Houston", ranked[2].Name);
	}

	[TestMethod]
	public void RankResults_ExactMatchBeforeStartsWithBeforeContains()
	{
		var results = new List<GeocodingResult>
		{
			new() { Name = "East Seattle" },
			new() { Name = "Seattle" },
			new() { Name = "Seattle Heights" },
		};

		var ranked = GeocodingService.RankResults("Seattle", results);

		Assert.AreEqual("Seattle", ranked[0].Name);
		Assert.AreEqual("Seattle Heights", ranked[1].Name);
		Assert.AreEqual("East Seattle", ranked[2].Name);
	}

	[TestMethod]
	public void RankResults_WithEmptyResults_ReturnsEmptyList()
	{
		var ranked = GeocodingService.RankResults("Seattle", []);

		Assert.AreEqual(0, ranked.Count);
	}

	[TestMethod]
	public async Task SearchLocationAsync_WhenAlreadyCanceled_ReturnsEmptyWithoutCallingApi()
	{
		var callCount = 0;
		var handler = new CountingHttpHandler(_ =>
		{
			callCount++;
			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("[]"),
			};
		});

		using var service = new GeocodingService(handler);
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		var results = await service.SearchLocationAsync("Seattle, WA, US", cts.Token);

		Assert.AreEqual(0, results.Count);
		Assert.AreEqual(0, callCount, "No API calls should be made when cancellation is already requested.");
	}

	[TestMethod]
	public async Task SearchLocationAsync_LongCommaInput_CapsAttemptsAtMaxFallbackAttempts()
	{
		var callCount = 0;
		var handler = new CountingHttpHandler(_ =>
		{
			callCount++;
			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("[]"),
			};
		});

		using var service = new GeocodingService(handler);
		// 5 comma-separated segments would produce 5 attempts without the cap
		var results = await service.SearchLocationAsync("City, Region, Province, Country, Extra", CancellationToken.None);

		Assert.AreEqual(0, results.Count);
		Assert.IsTrue(callCount <= GeocodingService.MaxFallbackAttempts,
			$"Expected at most {GeocodingService.MaxFallbackAttempts} API calls, but got {callCount}.");
	}

}

/// <summary>
/// An <see cref="HttpMessageHandler"/> that delegates each request to a factory function,
/// allowing tests to control and count outgoing HTTP calls.
/// </summary>
internal sealed class CountingHttpHandler(
	Func<HttpRequestMessage, HttpResponseMessage> responseFactory,
	Action<HttpRequestMessage>? onRequest = null) : HttpMessageHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		onRequest?.Invoke(request);
		return Task.FromResult(responseFactory(request));
	}
}
