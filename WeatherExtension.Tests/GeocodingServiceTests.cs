// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class GeocodingServiceTests
{
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
}
