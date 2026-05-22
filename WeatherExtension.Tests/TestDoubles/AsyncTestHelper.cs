// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CmdPal.Ext.Weather.UnitTests.TestDoubles;

internal static class AsyncTestHelper
{
	public static async Task<T> WaitUntilAsync<T>(
		Func<T> probe,
		Func<T, bool> predicate,
		int timeoutMs = 5000,
		int pollIntervalMs = 50)
	{
		var deadline = Environment.TickCount64 + timeoutMs;
		while (Environment.TickCount64 < deadline)
		{
			var value = probe();
			if (predicate(value))
			{
				return value;
			}

			await Task.Delay(pollIntervalMs);
		}

		Assert.Fail($"Condition not met within {timeoutMs} ms.");
		return probe();
	}
}
