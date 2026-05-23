// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace BaldBeardedBuilder.WeatherExtension;

internal static class WeatherLogger
{
	internal static void LogToHost(MessageState state, string message)
	{
		ExtensionHost.LogMessage(new LogMessage
		{
			Message = message,
			State = state,
		});

		RollingFileLogger.Instance.Log(MapLevel(state), message);
	}

	private static LogLevel MapLevel(MessageState state) => state switch
	{
		MessageState.Error => LogLevel.Error,
		MessageState.Warning => LogLevel.Warning,
		_ => LogLevel.Info,
	};
}
