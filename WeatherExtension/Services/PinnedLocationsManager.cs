// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BaldBeardedBuilder.WeatherExtension;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CmdPal.Ext.Weather.Models;
using System.Text.Json;

namespace Microsoft.CmdPal.Ext.Weather.Services;

public sealed class PinnedLocationsManager
{
	private readonly string _filePath;
	private List<PinnedLocation> _pinnedLocations = [];
	private readonly Lock _sync = new();

	public event EventHandler? PinnedLocationsChanged;

	public PinnedLocationsManager()
	{
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var directory = Path.Combine(localAppData, "Microsoft.CmdPal");
		Directory.CreateDirectory(directory);
		_filePath = Path.Combine(directory, "pinned-weather-locations.json");
		LoadFromFile();
	}

	internal PinnedLocationsManager(string filePath)
	{
		_filePath = filePath;
		LoadFromFile();
	}

	public void Pin(GeocodingResult location)
	{
		lock (_sync)
		{
			if (_pinnedLocations.Any(p => Math.Abs(p.Latitude - location.Latitude) < 0.01 &&
										  Math.Abs(p.Longitude - location.Longitude) < 0.01))
			{
				return;
			}

			_pinnedLocations.Add(new PinnedLocation
			{
				Latitude = location.Latitude,
				Longitude = location.Longitude,
				DisplayName = location.DisplayName,
				Name = location.Name,
				Admin1 = location.Admin1,
				Country = location.Country,
			});

			SaveToFile();
		}

		PinnedLocationsChanged?.Invoke(this, EventArgs.Empty);
	}

	public void Unpin(GeocodingResult location)
	{
		bool removed;
		lock (_sync)
		{
			var toRemove = _pinnedLocations.FirstOrDefault(p =>
				Math.Abs(p.Latitude - location.Latitude) < 0.01 &&
				Math.Abs(p.Longitude - location.Longitude) < 0.01);

			removed = toRemove != null && _pinnedLocations.Remove(toRemove);

			if (removed)
			{
				SaveToFile();
			}
		}

		if (removed)
		{
			PinnedLocationsChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public bool IsPinned(GeocodingResult location)
	{
		lock (_sync)
		{
			return _pinnedLocations.Any(p =>
				Math.Abs(p.Latitude - location.Latitude) < 0.01 &&
				Math.Abs(p.Longitude - location.Longitude) < 0.01);
		}
	}

	public List<PinnedLocation> GetPinnedLocations()
	{
		lock (_sync)
		{
			return new List<PinnedLocation>(_pinnedLocations);
		}
	}

	private void LoadFromFile()
	{
		try
		{
			if (File.Exists(_filePath))
			{
				var json = File.ReadAllText(_filePath);
				var locations = JsonSerializer.Deserialize<List<PinnedLocation>>(json, WeatherJsonContext.Default.ListPinnedLocation);
				if (locations != null)
				{
					_pinnedLocations = locations;
				}
			}
		}
		catch (Exception ex)
		{
			WeatherLogger.LogToHost(
				MessageState.Error,
				$"Failed to load pinned locations: {ex.Message}");
		}
	}

	private void SaveToFile()
	{
		try
		{
			var json = JsonSerializer.Serialize(_pinnedLocations, WeatherJsonContext.Default.ListPinnedLocation);
			File.WriteAllText(_filePath, json);
		}
		catch (Exception ex)
		{
			WeatherLogger.LogToHost(
				MessageState.Error,
				$"Failed to save pinned locations: {ex.Message}");
		}
	}
}
