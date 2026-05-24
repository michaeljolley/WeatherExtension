// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BaldBeardedBuilder.WeatherExtension;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;

namespace Microsoft.CmdPal.Ext.Weather.Commands;

internal sealed partial class OpenGitHubIssuesCommand : InvokableCommand
{
    private const string GitHubIssuesUrl = "https://github.com/michaeljolley/WeatherExtension/issues/new";

    public OpenGitHubIssuesCommand()
    {
        Name = Resources.bug_report_open_github;
        Icon = new IconInfo("\uE8A7"); // Link icon
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _ = Launcher.LaunchUriAsync(new Uri(GitHubIssuesUrl));
        }
        catch (Exception ex)
        {
            WeatherLogger.LogToHost(MessageState.Error, $"OpenGitHubIssuesCommand failed: {ex.Message}");
        }

        return CommandResult.KeepOpen();
    }
}
