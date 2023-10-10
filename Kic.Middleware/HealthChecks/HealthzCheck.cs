// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kic.Middleware;

/// <summary>
/// Health Check that supports dotnet IHeathCheck
/// </summary>
public class HealthzCheck
{
    public const string TimeoutMessage = "Request exceeded expected duration";

    [JsonPropertyName("status")]
    public HealthStatus Status { get; set; } = HealthStatus.Healthy;

    [JsonPropertyName("componentId")]
    public string ComponentId { get; set; } = string.Empty;

    [JsonPropertyName("componentType")]
    public string ComponentType { get; set; } = "datastore";

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(0);

    [JsonPropertyName("targetDuration")]
    public TimeSpan TargetDuration { get; set; } = TimeSpan.FromSeconds(0);

    [JsonPropertyName("time")]
    public string Time { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    // Build the Healthz Check
    public static HealthzCheck BuildHealthzCheck(string uri, TimeSpan duration, double targetDurationMs, Exception? ex = null, Dictionary<string, object>? data = null, string testName = "")
    {
        // create the result
        HealthzCheck result = new ()
        {
            Endpoint = uri,
            Duration = duration,
            TargetDuration = new TimeSpan(0, 0, 0, 0, (int)targetDurationMs),
            ComponentId = testName,
        };

        // check duration
        if (result.Duration.TotalMilliseconds > targetDurationMs)
        {
            result.Status = HealthStatus.Degraded;
            result.Message = TimeoutMessage;
        }

        // add the exception
        if (ex is not null)
        {
            result.Status = HealthStatus.Unhealthy;
            result.Message = ex.Message;
        }

        // add the results to the dictionary
        if (data is not null && !string.IsNullOrWhiteSpace(testName))
        {
            data.Add(testName + ":responseTime", result);
        }

        return result;
    }
}
