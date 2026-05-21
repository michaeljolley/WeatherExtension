// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class PinnedLocationTests
{
	[TestMethod]
	public void ToGeocodingResult_PreservesCoordinatesAndMetadata()
	{
		var pinned = new PinnedLocation
		{
			Latitude = 41.0082,
			Longitude = 28.9784,
			Name = "Istanbul",
			Admin1 = "Istanbul",
			Country = "Turkey",
		};

		var result = pinned.ToGeocodingResult();

		Assert.AreEqual(41.0082, result.Latitude);
		Assert.AreEqual(28.9784, result.Longitude);
		Assert.AreEqual("Istanbul", result.Name);
		Assert.AreEqual("Istanbul", result.Admin1);
		Assert.AreEqual("Turkey", result.Country);
		Assert.AreNotEqual(0, result.Id);
	}

	[TestMethod]
	public void ToGeocodingResult_DifferentCoordinates_ProduceDifferentIds()
	{
		var a = new PinnedLocation { Latitude = 41.0, Longitude = 29.0, Name = "A" }.ToGeocodingResult();
		var b = new PinnedLocation { Latitude = 40.9, Longitude = 29.1, Name = "B" }.ToGeocodingResult();

		Assert.AreNotEqual(a.Id, b.Id);
	}
}
