// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Models;
using Microsoft.CmdPal.Ext.Weather.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.Commands;

internal sealed partial class UnpinFromDockCommand : InvokableCommand
{
	private readonly GeocodingResult _location;
	private readonly PinnedLocationsManager _pinnedLocationsManager;

	public UnpinFromDockCommand(GeocodingResult location, PinnedLocationsManager pinnedLocationsManager)
	{
		_location = location;
		_pinnedLocationsManager = pinnedLocationsManager;
		Name = "Unpin from Dock";
		Icon = new IconInfo("📍");
	}

	public override string Id => "com.baldbeardedbuilder.cmdpal.weather.unpinFromDock";

	public override ICommandResult Invoke()
	{
		_pinnedLocationsManager.Unpin(_location);
		return CommandResult.KeepOpen();
	}
}
