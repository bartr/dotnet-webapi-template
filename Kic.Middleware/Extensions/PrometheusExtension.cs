// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace Kic.Middleware;

/// <summary>
/// Registers aspnet middleware handler that handles /version
/// </summary>
public static class PrometheusExtension
{
    /// <summary>
    /// Middleware extension method to handle /metrics request
    /// </summary>
    /// <param name="builder">this IApplicationBuilder</param>
    /// <param name="appName">application name</param>
    /// <param name="region">user defined Region</param>
    /// <param name="zone">user defined Zone</param>
    /// <returns>IApplicationBuilder</returns>
    public static IApplicationBuilder UsePrometheus(this IApplicationBuilder builder, string appName, string region, string zone)
    {
        Histogram requestHistogram = Metrics.CreateHistogram(
                $"{appName}_Duration",
                "Histogram of app request duration",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(1, 2, 10),
                    LabelNames = new string[] { "code", "mode", "region", "zone" },
                });

        Summary requestSummary = Metrics.CreateSummary(
                $"{appName}_Summary",
                "Summary of app request duration",
                new SummaryConfiguration
                {
                    SuppressInitialValue = true,
                    MaxAge = TimeSpan.FromMinutes(5),
                    Objectives = new List<QuantileEpsilonPair> { new QuantileEpsilonPair(.9, .0), new QuantileEpsilonPair(.95, .0), new QuantileEpsilonPair(.99, .0), new QuantileEpsilonPair(1.0, .0) },
                    LabelNames = new string[] { "code", "mode", "region", "zone" },
                });

        builder.Use(async Task (context, next) =>
        {
            DateTime dtStart = DateTime.Now;
            double duration = 0;

            // Invoke next handler
            if (next != null)
            {
                await next().ConfigureAwait(false);
            }

            await context.Response.CompleteAsync();

            string mode = "static";
            string path = string.IsNullOrEmpty(context.Request.Path.Value) ? string.Empty : context.Request.Path.Value;

            // api requests
            if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/v1"))
            {
                mode = "api";
            }

            // app health requests
            if (context.Request.Path.StartsWithSegments("/version") ||
                context.Request.Path.StartsWithSegments("/healthz") ||
                context.Request.Path.StartsWithSegments("/readyz"))
            {
                mode = "health";
            }

            // handle UI pages
            if (appName == "ResEdgeUi")
            {
                if (context.Request.Path == "/" ||
                    context.Request.Path.StartsWithSegments("/application") ||
                    context.Request.Path.StartsWithSegments("/applications") ||
                    context.Request.Path.StartsWithSegments("/cluster") ||
                    context.Request.Path.StartsWithSegments("/clusters") ||
                    context.Request.Path.StartsWithSegments("/group") ||
                    context.Request.Path.StartsWithSegments("/groups") ||
                    context.Request.Path.StartsWithSegments("/namespace") ||
                    context.Request.Path.StartsWithSegments("/namespaces") ||
                    context.Request.Path.StartsWithSegments("/policies") ||
                    context.Request.Path.StartsWithSegments("/policy") ||
                    context.Request.Path.StartsWithSegments("/user") ||
                    context.Request.Path.StartsWithSegments("/users"))
                {
                    mode = "page";
                }
            }

            // compute request duration
            duration = Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds, 2);

            // don't observe static requests
            if (mode == "static")
            {
                // observe Prom events
                requestHistogram.WithLabels(GetPrometheusCode(context.Response.StatusCode), mode, region, zone).Observe(duration);
                requestSummary.WithLabels(GetPrometheusCode(context.Response.StatusCode), mode, region, zone).Observe(duration);
            }
        });

        builder.UseMetricServer("/metrics");

        return builder;
    }

    // convert status code to string group for Prom metrics
    private static string GetPrometheusCode(int statusCode)
    {
        return statusCode >= 500 ? "Error" : statusCode == 429 ? "Retry" : statusCode >= 400 ? "Warn" : "OK";
    }
}
