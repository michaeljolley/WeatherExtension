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
	}
}
