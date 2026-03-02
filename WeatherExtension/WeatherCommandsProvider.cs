// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.DockBands;
using Microsoft.CmdPal.Ext.Weather.Pages;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather;

public sealed partial class WeatherCommandsProvider : CommandProvider
{
	private readonly WeatherSettingsManager _settingsManager = new();
	private readonly OpenMeteoService _weatherService = new();
	private readonly GeocodingService _geocodingService = new();
	private readonly WeatherBandCard _weatherContentPage;
	private readonly CurrentWeatherBand _currentWeatherBand;
	private readonly WeatherListPage _weatherPage;
	private readonly ICommandItem[] _topLevelItems;

	public WeatherCommandsProvider()
	{
		Id = "com.baldbeardedbuilder.cmdpal.weather";
		DisplayName = Resources.plugin_name;
		Icon = Icons.WeatherIcon;
		Settings = _settingsManager.Settings;

		_weatherContentPage = new WeatherBandCard(_weatherService, _geocodingService, _settingsManager);
		_currentWeatherBand = new CurrentWeatherBand(_weatherService, _geocodingService, _settingsManager, _weatherContentPage);

		// Create main weather page
		_weatherPage = new WeatherListPage(_weatherService, _geocodingService, _settingsManager);

		_topLevelItems =
		[
			new CommandItem(_weatherPage)
			{
				Icon = Icons.WeatherIcon,
				Title = Resources.plugin_name,
				MoreCommands = [new CommandContextItem(_settingsManager.Settings.SettingsPage)],
			},
		];
	}

	public override ICommandItem[] TopLevelCommands() => _topLevelItems;

	// DockBands API is not yet available in the published NuGet SDK.
	// Uncomment when the SDK includes WrappedDockItem and GetDockBands.
	/*
	public override ICommandItem[] GetDockBands()
	{
		var wrappedBand = new WrappedDockItem(
			[_currentWeatherBand],
			"com.baldbeardedbuilder.cmdpal.weather.dockBand",
			"Weather");

		return [wrappedBand];
	}
	*/

	public override void Dispose()
	{
		_weatherPage?.Dispose();
		_weatherContentPage?.Dispose();
		_currentWeatherBand?.Dispose();
		_weatherService?.Dispose();
		_geocodingService?.Dispose();
		base.Dispose();
		GC.SuppressFinalize(this);
	}
}
