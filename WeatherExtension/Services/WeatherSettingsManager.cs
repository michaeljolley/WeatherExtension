// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.Services;

public sealed class WeatherSettingsManager : JsonSettingsManager
{
    private const string Namespace = "weather";

    private static string Namespaced(string propertyName) => $"{Namespace}.{propertyName}";

    private readonly TextSetting _defaultLocation = new(
        Namespaced(nameof(DefaultLocation)),
        Resources.default_location_title,
        Resources.default_location_description,
        "98101")
    {
        Placeholder = Resources.default_location_placeholder,
    };

    private readonly ChoiceSetSetting _temperatureUnit = new(
        Namespaced(nameof(TemperatureUnit)),
        Resources.temperature_unit_title,
        Resources.temperature_unit_description,
        [
            new ChoiceSetSetting.Choice(Resources.celsius, "celsius"),
            new ChoiceSetSetting.Choice(Resources.fahrenheit, "fahrenheit"),
        ]);

    private readonly ChoiceSetSetting _windSpeedUnit = new(
        Namespaced(nameof(WindSpeedUnit)),
        Resources.wind_speed_unit_title,
        Resources.wind_speed_unit_description,
        [
            new ChoiceSetSetting.Choice(Resources.kmh, "kmh"),
            new ChoiceSetSetting.Choice(Resources.mph, "mph"),
        ]);

    private readonly ToggleSetting _showForecast = new(
        Namespaced(nameof(ShowForecast)),
        Resources.show_forecast_title,
        Resources.show_forecast_description,
        true);

    private readonly ChoiceSetSetting _updateInterval = new(
        Namespaced(nameof(UpdateIntervalMinutes)),
        Resources.update_interval_title,
        Resources.update_interval_description,
        [
            new ChoiceSetSetting.Choice(Resources.ten_minutes, "10"),
            new ChoiceSetSetting.Choice(Resources.fifteen_minutes, "15"),
            new ChoiceSetSetting.Choice(Resources.thirty_minutes, "30"),
            new ChoiceSetSetting.Choice(Resources.sixty_minutes, "60"),
        ]);

    public string DefaultLocation => _defaultLocation.Value ?? "98101";

    public string TemperatureUnit => _temperatureUnit.Value ?? "celsius";

    public string WindSpeedUnit => _windSpeedUnit.Value ?? "kmh";

    public bool ShowForecast => _showForecast.Value;

    public int UpdateIntervalMinutes => int.TryParse(_updateInterval.Value, out var value) && value > 0 ? value : 15;

    public WeatherSettingsManager()
    {
        FilePath = SettingsJsonPath();

        Settings.Add(_defaultLocation);
        Settings.Add(_temperatureUnit);
        Settings.Add(_windSpeedUnit);
        Settings.Add(_showForecast);
        Settings.Add(_updateInterval);

        LoadSettings();

        Settings.SettingsChanged += (_, _) => SaveSettings();
    }

    internal WeatherSettingsManager(string filePath)
    {
        FilePath = filePath;

        Settings.Add(_defaultLocation);
        Settings.Add(_temperatureUnit);
        Settings.Add(_windSpeedUnit);
        Settings.Add(_showForecast);
        Settings.Add(_updateInterval);

        LoadSettings();

        Settings.SettingsChanged += (_, _) => SaveSettings();
    }

    private static string SettingsJsonPath()
    {
        // Standalone implementation - use LocalApplicationData for settings
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(localAppData, "Microsoft.CmdPal");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "settings.json");
    }
}
