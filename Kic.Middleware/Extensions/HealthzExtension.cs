// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Kic.Middleware;

/// <summary>
/// Registers aspnet middleware handler that handles /healthz
/// </summary>
public static class HealthzExtension
{
    private static readonly ILogger Logger = Log.Logger;
    private static IHealthCheck? check = null;

    /// <summary>
    /// Middleware extension method to handle /healthz request
    /// </summary>
    /// <param name="builder">this IApplicationBuilder</param>
    /// <param name="healthCheck">Health Check instance to use</param>
    /// <returns>IApplicationBuilder</returns>
    public static IApplicationBuilder UseHealthz(this IApplicationBuilder builder, IHealthCheck healthCheck)
    {
        check = healthCheck;

        // implement the middleware
        builder.Use(async (context, next) =>
        {
            string match = "/healthz";
            string path = context.Request.Path.ToString().ToLowerInvariant();

            // matches /healthz
            if (path == match || path == $"{match}/ietf")
            {
                // return the healthz info
                context.Response.ContentType = "text/plain";

                try
                {
                    HealthCheckResult res = await RunHealthCheck();

                    if (path == match)
                    {
                        await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(IetfHealthCheck.ToIetfStatus(res.Status))).ConfigureAwait(false);
                    }
                    else
                    {
                        string? serviceId = string.Empty;

                        if (res.Data is not null && res.Data.ContainsKey("serviceId"))
                        {
                            serviceId = res.Data["serviceId"].ToString();
                            serviceId ??= string.Empty;
                        }

                        await IetfHealthCheck.Writer(context, res, serviceId, TimeSpan.FromSeconds(0));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "{@code}", LogCodes.HealthcheckFailed);
                    await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(IetfHealthCheck.ToIetfStatus(HealthStatus.Unhealthy))).ConfigureAwait(false);
                }
            }
            else
            {
                // not a match, so call next middleware handler
                await next().ConfigureAwait(false);
            }
        });

        return builder;
    }

    private static async Task<HealthCheckResult> RunHealthCheck()
    {
        Log.Logger.Information("{@code}", LogCodes.HealthcheckStart);

        HealthCheckResult res;

        if (check is null)
        {
            res = new HealthCheckResult(HealthStatus.Unhealthy, "HealthCheck function is null", new Exception("HealthCheck function is null"));
        }
        else
        {
            // run health check
            res = await check.CheckHealthAsync(new());
        }

        // log status
        if (res.Exception is not null)
        {
            Logger.Error(res.Exception, "{@code}{@msg}", LogCodes.HealthcheckFailed, IetfHealthCheck.ToIetfStatus(res.Status));
        }
        else
        {
            switch (res.Status)
            {
                case HealthStatus.Healthy:
                    Logger.Information("{@code}{@msg}", LogCodes.HealthcheckComplete, IetfHealthCheck.ToIetfStatus(res.Status));
                    break;
                case HealthStatus.Degraded:
                    Logger.Warning("{@code}{@msg}", LogCodes.HealthcheckComplete, IetfHealthCheck.ToIetfStatus(res.Status));
                    break;
                case HealthStatus.Unhealthy:
                    Logger.Error("{@code}{@msg}", LogCodes.HealthcheckComplete, IetfHealthCheck.ToIetfStatus(res.Status));
                    break;
            }
        }

        return res;
    }
}
