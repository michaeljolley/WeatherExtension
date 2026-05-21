// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Reflection;
using Microsoft.CmdPal.Ext.Weather;
using Microsoft.CmdPal.Ext.Weather.DockBands;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.CommandPalette.Extensions;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

/// <summary>
/// Regression tests for dock-band cache behavior when favorites change.
/// </summary>
[TestClass]
public class WeatherCommandsProviderFavoritesTests
{
	private static readonly BindingFlags InstanceNonPublic =
		BindingFlags.Instance | BindingFlags.NonPublic;

	[TestMethod]
	public void GetDockBands_AfterUnfavoriteAndRefavorite_DoesNotReuseDisposedBand()
	{
		var favoritesPath = Path.Combine(
			Path.GetTempPath(),
			$"weather-provider-fav-{Guid.NewGuid()}.json");

		try
		{
			var favorites = new FavoritesManager(favoritesPath);
			var seattle = new GeocodingResult { Name = "Seattle", Latitude = 47.6, Longitude = -122.3 };
			var portland = new GeocodingResult { Name = "Portland", Latitude = 45.5, Longitude = -122.7 };

			favorites.Favorite(seattle);
			favorites.Favorite(portland);

			using var provider = new WeatherCommandsProvider();
			InjectFavoritesManager(provider, favorites);

			var firstGeneration = GetDockBands(provider);
			Assert.AreEqual(2, firstGeneration.Length);

			var portlandBand = GetBandForLocation(provider, portland);
			Assert.IsFalse(portlandBand.IsDisposed);

			favorites.Unfavorite(portland);
			InvokeFavoritesChanged(provider);

			var afterUnfavorite = GetDockBands(provider);
			Assert.AreEqual(1, afterUnfavorite.Length);
			Assert.IsTrue(portlandBand.IsDisposed);

			favorites.Favorite(portland);
			InvokeFavoritesChanged(provider);

			var afterRefavorite = GetDockBands(provider);
			Assert.AreEqual(2, afterRefavorite.Length);

			var newPortlandBand = GetBandForLocation(provider, portland);
			Assert.IsFalse(newPortlandBand.IsDisposed);
			Assert.AreNotSame(portlandBand, newPortlandBand,
				"Re-favoriting should create a fresh band instance, not reuse a disposed one.");
		}
		finally
		{
			if (File.Exists(favoritesPath))
			{
				File.Delete(favoritesPath);
			}
		}
	}

	[TestMethod]
	public void GetDockBands_DuplicateCoordinatesAtF4Precision_ReturnsSingleBand()
	{
		var favoritesPath = Path.Combine(
			Path.GetTempPath(),
			$"weather-provider-dup-{Guid.NewGuid()}.json");

		try
		{
			// Write two entries that share the same F4 band key but are far enough
			// apart that Favorite()'s 0.01° duplicate threshold would block a second Add.
			File.WriteAllText(
				favoritesPath,
				"""
				[
				  {"latitude":41.0082,"longitude":28.9784,"name":"A","displayName":"A"},
				  {"latitude":41.00824,"longitude":28.97844,"name":"B","displayName":"B"}
				]
				""");

			var favorites = new FavoritesManager(favoritesPath);

			using var provider = new WeatherCommandsProvider();
			InjectFavoritesManager(provider, favorites);

			var bands = GetDockBands(provider);

			Assert.AreEqual(1, bands.Length, "F4 band keys collide — only one dock band should surface.");
			Assert.AreEqual(2, favorites.GetFavorites().Count, "Both favorites remain stored.");
		}
		finally
		{
			if (File.Exists(favoritesPath))
			{
				File.Delete(favoritesPath);
			}
		}
	}

	private static void InjectFavoritesManager(WeatherCommandsProvider provider, FavoritesManager manager)
	{
		var field = typeof(WeatherCommandsProvider).GetField(
			"_favoritesManager",
			InstanceNonPublic);
		Assert.IsNotNull(field);
		field!.SetValue(provider, manager);
	}

	private static void InvokeFavoritesChanged(WeatherCommandsProvider provider)
	{
		var method = typeof(WeatherCommandsProvider).GetMethod(
			"OnFavoritesChanged",
			InstanceNonPublic);
		Assert.IsNotNull(method);
		method!.Invoke(provider, [provider, EventArgs.Empty]);
	}

	private static ICommandItem[] GetDockBands(WeatherCommandsProvider provider)
		=> provider.GetDockBands();

	private static PinnedWeatherBand GetBandForLocation(
		WeatherCommandsProvider provider,
		GeocodingResult location)
	{
		var bandsField = typeof(WeatherCommandsProvider).GetField(
			"_bandsByKey",
			InstanceNonPublic);
		Assert.IsNotNull(bandsField);

		var bands = bandsField!.GetValue(provider);
		Assert.IsNotNull(bands);

		var key = FormattableString.Invariant($"{location.Latitude:F4}_{location.Longitude:F4}");
		var tryGetValue = bands!.GetType().GetMethod("TryGetValue")!;
		var args = new object?[] { key, null };
		Assert.IsTrue((bool)tryGetValue.Invoke(bands, args)!, $"Band cache missing key {key}.");

		var entry = args[1];
		Assert.IsNotNull(entry);

		var bandProperty = entry!.GetType().GetProperty("Band", BindingFlags.Instance | BindingFlags.Public);
		Assert.IsNotNull(bandProperty);
		return (PinnedWeatherBand)bandProperty!.GetValue(entry)!;
	}
}
