// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class JsonFileStoreTests
{
	private string _tempFilePath = string.Empty;

	[TestInitialize]
	public void Setup()
	{
		_tempFilePath = Path.Combine(Path.GetTempPath(), $"weather-json-store-{Guid.NewGuid()}.json");
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (File.Exists(_tempFilePath))
		{
			File.Delete(_tempFilePath);
		}
	}

	[TestMethod]
	public void Add_NewItem_PersistsAndReturnsTrue()
	{
		var store = CreateStore();
		var added = store.Add(
			new PinnedLocation { Name = "Seattle", Latitude = 47.6, Longitude = -122.3 },
			p => p.Name == "Seattle");

		Assert.IsTrue(added);
		Assert.AreEqual(1, store.GetAll().Count);
		Assert.IsTrue(File.Exists(_tempFilePath));
	}

	[TestMethod]
	public void Add_Duplicate_ReturnsFalse()
	{
		var store = CreateStore();
		var item = new PinnedLocation { Name = "Seattle", Latitude = 47.6, Longitude = -122.3 };

		Assert.IsTrue(store.Add(item, p => p.Name == "Seattle"));
		Assert.IsFalse(store.Add(item, p => p.Name == "Seattle"));
		Assert.AreEqual(1, store.GetAll().Count);
	}

	[TestMethod]
	public void Remove_ExistingItem_ReturnsTrue()
	{
		var store = CreateStore();
		store.Add(new PinnedLocation { Name = "A", Latitude = 1, Longitude = 1 }, _ => false);
		store.Add(new PinnedLocation { Name = "B", Latitude = 2, Longitude = 2 }, _ => false);

		var removed = store.Remove(p => p.Name == "A");

		Assert.IsTrue(removed);
		Assert.AreEqual(1, store.GetAll().Count);
		Assert.AreEqual("B", store.GetAll()[0].Name);
	}

	[TestMethod]
	public void GetAll_ReturnsCopy_NotLiveList()
	{
		var store = CreateStore();
		store.Add(new PinnedLocation { Name = "Seattle", Latitude = 47.6, Longitude = -122.3 }, _ => false);

		var snapshot = store.GetAll();
		snapshot.Add(new PinnedLocation { Name = "Extra", Latitude = 0, Longitude = 0 });

		Assert.AreEqual(1, store.GetAll().Count);
	}

	[TestMethod]
	public void ReplaceAll_ChangedContents_ReturnsTrue()
	{
		var store = CreateStore();
		store.Add(new PinnedLocation { Name = "Old", Latitude = 1, Longitude = 1 }, _ => false);

		var replaced = store.ReplaceAll(
		[
			new PinnedLocation { Name = "New", Latitude = 2, Longitude = 2 },
		]);

		Assert.IsTrue(replaced);
		Assert.AreEqual("New", store.GetAll()[0].Name);
	}

	[TestMethod]
	public void ReplaceAll_SameContents_ReturnsFalse()
	{
		var store = CreateStore();
		var items =
			new List<PinnedLocation> { new() { Name = "Same", Latitude = 1, Longitude = 1 } };
		store.Add(items[0], _ => false);

		var replaced = store.ReplaceAll(items);

		Assert.IsFalse(replaced);
	}

	[TestMethod]
	public void Constructor_LoadsPersistedJson()
	{
		File.WriteAllText(
			_tempFilePath,
			"""[{"latitude":41.0,"longitude":29.0,"name":"Istanbul","displayName":"Istanbul, TR"}]""");

		var store = CreateStore();

		Assert.AreEqual(1, store.GetAll().Count);
		Assert.AreEqual("Istanbul", store.GetAll()[0].Name);
	}

	private JsonFileStore<PinnedLocation> CreateStore()
		=> new(_tempFilePath, WeatherJsonContext.Default.ListPinnedLocation, "test items");
}
