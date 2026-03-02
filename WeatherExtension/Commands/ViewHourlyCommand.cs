// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Pages;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.Commands;

internal sealed partial class ViewHourlyCommand : InvokableCommand
{
	private readonly GeocodingResult _location;
	private readonly IWeatherService _weatherService;
	private readonly WeatherSettingsManager _settingsManager;

	public ViewHourlyCommand(
		GeocodingResult location,
		IWeatherService weatherService,
		WeatherSettingsManager settingsManager)
	{
		ArgumentNullException.ThrowIfNull(location);
		ArgumentNullException.ThrowIfNull(weatherService);
		ArgumentNullException.ThrowIfNull(settingsManager);

		_location = location;
		_weatherService = weatherService;
		_settingsManager = settingsManager;

		Name = "View Hourly";
	}

	public override string Id => $"com.baldbeardedbuilder.cmdpal.weather.viewhourly.{_location.Id}";

	public override IconInfo Icon => Icons.WeatherIcon;

	public override ICommandResult Invoke()
	{
		var page = new HourlyForecastPage(_location, _weatherService, _settingsManager);
		return CommandResult.GoToPage(new GoToPageArgs { PageId = page.Id });
	}
}
