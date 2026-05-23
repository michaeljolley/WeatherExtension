// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO.Compression;
using BaldBeardedBuilder.WeatherExtension;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.Commands;

internal sealed partial class SaveLogsCommand : InvokableCommand
{
    public SaveLogsCommand()
    {
        Name = Resources.bug_report_save_logs;
        Icon = new IconInfo("\uE74E"); // Save icon
    }

    public override ICommandResult Invoke()
    {
        try
        {
            RollingFileLogger.Instance.Flush();

            var logDir = RollingFileLogger.Instance.LogDirectory;
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var zipName = $"WeatherExtension-Logs-{DateTime.Now:yyyy-MM-dd}.zip";
            var zipPath = Path.Combine(desktop, zipName);

            // Remove any existing zip with the same name so CreateFromDirectory doesn't throw.
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            // Add only *.log files — skip any unrelated files that may be in the directory.
            if (Directory.Exists(logDir))
            {
                using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                foreach (var logFile in Directory.EnumerateFiles(logDir, "*.log"))
                {
                    archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile), CompressionLevel.Optimal);
                }
            }

            WeatherLogger.LogToHost(MessageState.Info, $"Logs saved to: {zipPath}");
        }
        catch (Exception ex)
        {
            WeatherLogger.LogToHost(MessageState.Error, $"SaveLogsCommand failed: {ex.Message}");
        }

        return CommandResult.KeepOpen();
    }
}
