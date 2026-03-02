// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CmdPal.Ext.Weather.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.Commands;

internal sealed partial class RefreshWeatherCommand : InvokableCommand
{
    private readonly WeatherListPage _page;

    public RefreshWeatherCommand(WeatherListPage page)
    {
        _page = page;
        Name = "Refresh";
    }

    public override string Id => "com.baldbeardedbuilder.cmdpal.weather.refresh";

    public override IconInfo Icon => Icons.WeatherIcon;

    public override ICommandResult Invoke()
    {
        _page.RefreshWeather();
        return CommandResult.KeepOpen();
    }
}
