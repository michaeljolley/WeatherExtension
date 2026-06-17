// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;
using System.Threading;
using BaldBeardedBuilder.WeatherExtension;

namespace WeatherExtension;

public class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
        {
            global::Shmuelie.WinRTServer.ComServer server = new();

            ManualResetEvent extensionDisposedEvent = new(false);
            ShutdownCoordinator? coordinator = null;
            
            try
            {
                // We are instantiating an extension instance once above, and returning it every time the callback in RegisterExtension below is called.
                // This makes sure that only one instance of WeatherExtension is alive, which is returned every time the host asks for the IExtension object.
                // If you want to instantiate a new instance each time the host asks, create the new instance inside the delegate.
                WeatherExtension extensionInstance = new(extensionDisposedEvent);
                server.RegisterClass<WeatherExtension, IExtension>(() => extensionInstance);
                
                // Set up graceful shutdown signaling from external processes (e.g., uninstaller)
                coordinator = new ShutdownCoordinator();
                
                server.Start();
                
                // Wait for either normal disposal (Command Palette shutdown) or external shutdown signal (uninstall)
                WaitHandle[] waitHandles = [extensionDisposedEvent, coordinator.ShutdownHandle];
                int signalledIndex = WaitHandle.WaitAny(waitHandles);
                
                // Log which shutdown path was taken
                if (signalledIndex == 1)
                {
                    WeatherLogger.LogToHost(
                        MessageState.Info,
                        "External shutdown signal received; initiating graceful shutdown");
                    
                    // If external shutdown signal fired (not normal disposal),
                    // manually trigger disposal to clean up resources
                    extensionInstance.Dispose();
                }
                
                // Set up watchdog timer: if shutdown takes too long, force exit
                using var watchdog = new Timer(
                    _ =>
                    {
                        WeatherLogger.LogToHost(
                            MessageState.Error,
                            "Shutdown watchdog timeout; forcing process exit");
                        Environment.Exit(1);
                    },
                    state: null,
                    dueTime: TimeSpan.FromSeconds(5),
                    period: Timeout.InfiniteTimeSpan);
                
                // Wait for extension disposal to complete
                extensionDisposedEvent.WaitOne();
                
                // Cancel the watchdog timer since shutdown completed in time
                watchdog.Change(Timeout.Infinite, Timeout.Infinite);
                
                server.Stop();
            }
            catch (Exception ex)
            {
                WeatherLogger.LogToHost(
                    MessageState.Error,
                    $"Fatal error during shutdown coordination: {ex.Message}");
            }
            finally
            {
                coordinator?.Dispose();
                extensionDisposedEvent.Dispose();
                server?.UnsafeDispose();
            }
        }
        else
        {
            Console.WriteLine("Not being launched as a Extension... exiting.");
        }
    }
}
