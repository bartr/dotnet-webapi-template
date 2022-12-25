// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace CSApp;

/// <summary>
/// Main application class
/// </summary>
public partial class Program
{
    public static Config Config { get; } = new Config();

    // main entry point
    public static int Main(string[] args)
    {
        DisplayAsciiArt(args);

        // build the System.CommandLine.RootCommand
        RootCommand root = BuildRootCommand();
        root.Handler = CommandHandler.Create<Config>(RunApp);

        // run the app
        return root.Invoke(args);
    }

    // create a CancellationTokenSource that cancels on ctl-c or sigterm
    private static CancellationTokenSource SetupSigTermHandler(WebApplication host)
    {
        CancellationTokenSource ctCancel = new ();

        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            ctCancel.Cancel();

            // logger.LogInformation("Shutdown", "Shutting Down ...");

            // trigger graceful shutdown for the webhost
            // force shutdown after timeout, defined in UseShutdownTimeout within BuildHost() method
            await host.StopAsync().ConfigureAwait(false);

            // end the app
            Environment.Exit(0);
        };

        return ctCancel;
    }

    // display Ascii Art
    private static void DisplayAsciiArt(string[] args)
    {
        if (args != null)
        {
            ReadOnlySpan<string> cmd = new (args);

            if (!cmd.Contains("--version") &&
                (cmd.Contains("-h") ||
                cmd.Contains("--help") ||
                cmd.Contains("--dry-run")))
            {
                string file = "ascii-art.txt";

                try
                {
                    if (File.Exists(file))
                    {
                        string txt = File.ReadAllText(file);

                        if (!string.IsNullOrWhiteSpace(txt))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.WriteLine(txt);
                            Console.ResetColor();
                        }
                    }
                }
                catch
                {
                    // ignore any errors
                }
            }
        }
    }
}
