// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.CommandLine;
using System.CommandLine.Parsing;
using CSApp.Model;
using MC.Middleware;

namespace CSApp
{
    /// <summary>
    /// Main application class
    /// </summary>
    public partial class Program
    {
        // capture parse errors from env vars
        private static readonly List<string> EnvVarErrors = new ();

        /// <summary>
        /// Run the app
        /// </summary>
        /// <param name="config">command line config</param>
        /// <returns>status</returns>
        public static async Task<int> RunApp(Config config)
        {
            // start collecting CPU usage
            // CpuCounter.Start();

            try
            {
                // copy command line values
                Config.SetConfig(config);

                // copy data file
                DataFile.Reset();

                if (new Database() == null)
                {
                    Console.WriteLine("Error: Unable to read database");
                    return 1;
                }

                // LoadSecrets();

                WebApplication app = BuildHost();
                ConfigApp(app);

                if (app == null)
                {
                    return -1;
                }

                // display dry run message
                if (config.DryRun)
                {
                    return DoDryRun();
                }

                // setup sigterm handler
                CancellationTokenSource ctCancel = SetupSigTermHandler(app);

                // start the webserver
                Task w = app.RunAsync();

                Console.WriteLine($"Listening on port {config.Port}");

                // this doesn't return except on ctl-c or sigterm
                await w.ConfigureAwait(false);

                // if not cancelled, app exit 1
                return ctCancel.IsCancellationRequested ? 0 : 1;
            }
            catch (Exception ex)
            {
                // end app on error
                Console.WriteLine("Exception" + ex);

                return -1;
            }
        }

        /// <summary>
        /// Build the RootCommand for parsing
        /// </summary>
        /// <returns>RootCommand</returns>
        public static RootCommand BuildRootCommand()
        {
            RootCommand root = new ()
            {
                Name = "CSApp",
                Description = "CSApp",
                TreatUnmatchedTokensAsErrors = true,
            };

            // add the options
            root.AddOption(EnvVarOption(new string[] { "--port", "-p" }, "Listen Port", 8080, 1, (64 * 1024) - 1));
            root.AddOption(EnvVarOption(new string[] { "--zone", "-z" }, "Zone for log", "dev"));
            root.AddOption(EnvVarOption(new string[] { "--region", "-r" }, "Region for log", "dev"));
            root.AddOption(EnvVarOption(new string[] { "--log-level", "-l" }, "Log Level", LogLevel.Error));
            root.AddOption(EnvVarOption(new string[] { "--cache-duration" }, "Cache for duration (seconds)", 60, 1));

            // root.AddOption(EnvVarOption(new string[] { "--secrets-volume", "-v" }, "Secrets Volume Path", "secrets"));
            root.AddOption(new Option<bool>(new string[] { "--dry-run", "-d" }, "Validates configuration"));

            // validate dependencies
            root.AddValidator(ValidateDependencies);

            return root;
        }

        // validate combinations of parameters
        private static void ValidateDependencies(CommandResult result)
        {
            string msg = string.Empty;

            if (EnvVarErrors.Count > 0)
            {
                msg += string.Join('\n', EnvVarErrors) + '\n';
            }

            // return error message(s) or string.empty
            result.ErrorMessage = msg;
        }

        // insert env vars as default
        private static Option EnvVarOption<T>(string[] names, string description, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // this will throw on bad names
            string env = GetValueFromEnvironment(names, out string key);

            T value = defaultValue;

            // set default to environment value if set
            if (!string.IsNullOrWhiteSpace(env))
            {
                if (defaultValue.GetType().IsEnum)
                {
                    if (Enum.TryParse(defaultValue.GetType(), env, true, out object result))
                    {
                        value = (T)result;
                    }
                    else
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
                else
                {
                    try
                    {
                        value = (T)Convert.ChangeType(env, typeof(T));
                    }
                    catch
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
            }

            return new Option<T>(names, () => value, description);
        }

        // insert env vars as default with min val for ints
        private static Option<int> EnvVarOption(string[] names, string description, int defaultValue, int minValue, int? maxValue = null)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // this will throw on bad names
            string env = GetValueFromEnvironment(names, out string key);

            int value = defaultValue;

            // set default to environment value if set
            if (!string.IsNullOrWhiteSpace(env))
            {
                if (!int.TryParse(env, out value))
                {
                    EnvVarErrors.Add($"Environment variable {key} is invalid");
                }
            }

            Option<int> opt = new (names, () => value, description);

            opt.AddValidator((res) =>
            {
                string s = string.Empty;
                int val;

                try
                {
                    val = (int)res.GetValueOrDefault();

                    if (val < minValue)
                    {
                        s = $"{names[0]} must be >= {minValue}";
                    }
                }
                catch
                {
                }

                res.ErrorMessage = s;
            });

            if (maxValue != null)
            {
                opt.AddValidator((res) =>
                {
                    string s = string.Empty;
                    int val;

                    try
                    {
                        val = (int)res.GetValueOrDefault();

                        if (val > maxValue)
                        {
                            s = $"{names[0]} must be <= {maxValue}";
                        }
                    }
                    catch
                    {
                    }

                    res.ErrorMessage = s;
                });
            }

            return opt;
        }

        // check for environment variable value
        private static string GetValueFromEnvironment(string[] names, out string key)
        {
            if (names == null ||
                names.Length < 1 ||
                names[0].Trim().Length < 4)
            {
                throw new ArgumentNullException(nameof(names));
            }

            for (int i = 1; i < names.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(names[i]) ||
                    names[i].Length != 2 ||
                    names[i][0] != '-')
                {
                    throw new ArgumentException($"Invalid command line parameter at position {i}", nameof(names));
                }
            }

            key = names[0][2..].Trim().ToUpperInvariant().Replace('-', '_');

            return Environment.GetEnvironmentVariable(key);
        }

        // Display the dry run message
        private static int DoDryRun()
        {
            Console.WriteLine($"Version            {VersionExtension.Version}");

            // Console.WriteLine($"Secrets Volume     {Config.Secrets.Volume}");

            Console.WriteLine($"Region             {Config.Region}");
            Console.WriteLine($"Zone               {Config.Zone}");

            Console.WriteLine($"Log Level          {Config.LogLevel}");
            Console.WriteLine($"Request Log Level  {Config.RequestLogLevel}");

            // always return 0 (success)
            return 0;
        }
    }
}
