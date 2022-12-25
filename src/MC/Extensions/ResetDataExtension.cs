// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using CSApp;
using Microsoft.AspNetCore.Builder;

namespace MC.Middleware
{
    /// <summary>
    /// Registers aspnet middleware handler that handles /api/v1/resetdata
    /// </summary>
    public static class ResetDataExtension
    {
        // cached values
        private static byte[] responseBytes;

        /// <summary>
        /// Middleware extension method to handle /api/v1/resetdata
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseResetData(this IApplicationBuilder builder)
        {
            responseBytes ??= System.Text.Encoding.UTF8.GetBytes("Data Reset Successfully");

            // implement the middleware
            builder.Use(async (context, next) =>
            {
                string path = "/api/v1/resetdata";

                // matches /api/v1/resetdata
                if (context.Request.Path.ToString().Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    DataFile.Reset();

                    // return the version info
                    context.Response.ContentType = "text/plain";

                    await context.Response.Body.WriteAsync(responseBytes).ConfigureAwait(false);
                }
                else
                {
                    // not a match, so call next middleware handler
                    await next().ConfigureAwait(false);
                }
            });

            return builder;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "todo")]
    public class DataFile
    {
        public static bool Reset()
        {
            string src = Path.Join("cadl", "sampleData.json");

            try
            {
                if (!File.Exists(src))
                {
                    src = Path.Join("..", "cadl", "sampleData.json");
                }

                if (File.Exists(src))
                {
                    if (!Directory.Exists("data"))
                    {
                        Directory.CreateDirectory("data");
                    }

                    File.Copy(src, Path.Join("data", "data.json"), true);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"todo: ResetDataFile Exception: {ex.Message}");
            }

            return false;
        }
    }
}
