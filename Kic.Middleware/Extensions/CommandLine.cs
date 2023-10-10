// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using Kic.Middleware;

namespace Kic.Middleware;

/// <summary>
/// System.CommandLine extensions
/// </summary>
public partial class CommandLine
{
    // capture parse errors from env vars
    private static readonly List<string> EnvVarErrors = new ();

    // validate combinations of parameters
    public static void ValidateEnvVars(CommandResult result)
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
    public static Option EnvVarOption<T>(string[] names, string description, T defaultValue)
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
            if (defaultValue is not null && defaultValue.GetType().IsEnum)
            {
                if (Enum.TryParse(defaultValue.GetType(), env, true, out object? result) && result is not null)
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
    public static Option<int> EnvVarOption(string[] names, string description, int defaultValue, int minValue, int? maxValue = null)
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
                var obj = res.GetValueOrDefault();
                val = obj is not null ? (int)obj : 0;

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
                    var obj = res.GetValueOrDefault();
                    val = obj is not null ? (int)obj : 0;

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

        return Environment.GetEnvironmentVariable(key) ?? string.Empty;
    }
}
