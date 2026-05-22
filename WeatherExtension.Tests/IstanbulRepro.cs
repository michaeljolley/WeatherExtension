// Temporary repro test for Istanbul / Turkish character search.
// Will be removed once the bug is fixed.

using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class IstanbulReproTests
{
	private const string IstanbulNominatimJson = """
		[{"place_id":60160394,"licence":"Data","osm_type":"node","osm_id":1882099475,"lat":"41.0063810","lon":"28.9758715","class":"place","type":"city","place_rank":16,"importance":0.78,"addresstype":"city","name":"İstanbul","display_name":"İstanbul, Fatih, İstanbul, Marmara Bölgesi, 34122, Türkiye","address":{"city":"İstanbul","town":"Fatih","province":"İstanbul","ISO3166-2-lvl4":"TR-34","region":"Marmara Bölgesi","postcode":"34122","country":"Türkiye","country_code":"tr"},"boundingbox":["40.84","41.16","28.81","29.13"]}]
		""";

	[TestMethod]
	public async Task SearchLocationAsync_Istanbul_ReturnsResult()
	{
		var handler = new CountingHttpHandler(_ =>
			new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(IstanbulNominatimJson),
			});

		using var service = new GeocodingService(handler);
		var results = await service.SearchLocationAsync("istanbul", CancellationToken.None);

		Assert.IsTrue(results.Count > 0, "Istanbul search should return at least one result");
		Assert.AreEqual(41.0063810, results[0].Latitude, 0.001);
		Assert.AreEqual(28.9758715, results[0].Longitude, 0.001);
	}

	[TestMethod]
	public async Task SearchLocationAsync_TurkishPostalCode34122_ReturnsResult()
	{
		var handler = new CountingHttpHandler(_ =>
			new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(IstanbulNominatimJson),
			});

		using var service = new GeocodingService(handler);
		var results = await service.SearchLocationAsync("34122", CancellationToken.None);

		Assert.IsTrue(results.Count > 0, "Postal code 34122 should return a result");
	}
}
