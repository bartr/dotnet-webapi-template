// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Kic.Middleware;

/// <summary>
/// Registers aspnet middleware handler that handles /version
/// </summary>
public static class VersionExtension
{
    // cached values
    private static string version = string.Empty;
    private static string shortVersion = string.Empty;

    /// <summary>
    /// Gets the app version
    /// </summary>
    public static string Version
    {
        get
        {
            CacheVersion();
            return version;
        }
    }

    /// <summary>
    /// Gets the app short version
    /// </summary>
    public static string ShortVersion
    {
        get
        {
            CacheVersion();
            return ShortVersion;
        }
    }

    /// <summary>
    /// Middleware extension method to handle /version request
    /// </summary>
    /// <param name="builder">this IApplicationBuilder</param>
    /// <returns>IApplicationBuilder</returns>
    public static IApplicationBuilder UseVersion(this IApplicationBuilder builder)
    {
        CacheVersion();

        // implement the middleware
        builder.Use(async (context, next) =>
        {
            string path = "/version";

            // matches /version
            if (context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase))
            {
                // return the version info
                context.Response.ContentType = "text/plain";

                await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(version)).ConfigureAwait(false);
            }
            else
            {
                // not a match, so call next middleware handler
                await next().ConfigureAwait(false);
            }
        });

        return builder;
    }

    // Cache version values with reflection
    private static void CacheVersion()
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            var asm = Assembly.GetEntryAssembly();

            // cache the version info
            if (asm is not null && Attribute.GetCustomAttribute(asm, typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
            {
                version = v.InformationalVersion;
                shortVersion = version;

                if (version.Contains('-', StringComparison.OrdinalIgnoreCase))
                {
                    shortVersion = version[..version.IndexOf('-', StringComparison.OrdinalIgnoreCase)];
                }
            }
            else
            {
                version = "unknown";
                shortVersion = version;
            }
        }
    }
}
