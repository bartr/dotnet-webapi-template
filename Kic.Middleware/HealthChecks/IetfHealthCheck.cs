// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kic.Middleware;

/// <summary>
/// Health Check that supports IETF json
/// </summary>
public class IetfHealthCheck
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IetfHealthCheck"/> class.
    /// Create an IetfCheck from a HealthzCheck
    /// </summary>
    /// <param name="hzCheck">HealthzCheck</param>
    public IetfHealthCheck(HealthzCheck hzCheck)
    {
        if (hzCheck == null)
        {
            throw new ArgumentNullException(nameof(hzCheck));
        }

        Status = ToIetfStatus(hzCheck.Status);
        ComponentId = hzCheck.ComponentId;
        ComponentType = hzCheck.ComponentType;
        ObservedValue = Math.Round(hzCheck.Duration.TotalMilliseconds, 2);
        TargetValue = Math.Round(hzCheck.TargetDuration.TotalMilliseconds, 2);
        ObservedUnit = "ms";
        Time = hzCheck.Time;
        Message = hzCheck.Message;

        if (!string.IsNullOrEmpty(hzCheck.Endpoint))
        {
            AffectedEndpoints = new List<string> { hzCheck.Endpoint };
        }
    }

    public string Status { get; set; } = string.Empty;
    public string ComponentId { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string ObservedUnit { get; set; } = string.Empty;
    public double ObservedValue { get; set; }
    public double TargetValue { get; set; }
    public string Time { get; set; } = string.Empty;
    public List<string> AffectedEndpoints { get; } = new();
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Write the health check results as json
    /// </summary>
    /// <param name="httpContext">HttpContext</param>
    /// <param name="healthReport">HealthReport</param>
    /// <param name="serviceId">Service Id</param>
    /// <param name="description">Service Description</param>
    /// <returns>Task</returns>
    public static Task Writer(HttpContext httpContext, HealthReport healthReport, string serviceId, string? description)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        if (healthReport == null)
        {
            throw new ArgumentNullException(nameof(healthReport));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = string.Empty;
        }

        // config serialization
        JsonSerializerOptions jsonOptions = JsonOptions.GetDefault();

        // create the dictionaries
        Dictionary<string, object> result = new ();
        Dictionary<string, object> checks = new ();

        // add header values
        result.Add("status", ToIetfStatus(healthReport.Status));
        result.Add("serviceId", serviceId);
        result.Add("description", description);

        // add all the entries
        foreach (HealthReportEntry e in healthReport.Entries.Values)
        {
            // add all the data elements
            foreach (KeyValuePair<string, object> d in e.Data)
            {
                // transform HealthzCheck into IetfCheck
                if (d.Value is HealthzCheck r)
                {
                    // add to checks dictionary
                    checks.Add(d.Key, new IetfHealthCheck(r));
                }
                else
                {
                    // add to the main dictionary
                    result[d.Key] = d.Value;
                }
            }
        }

        // add the checks to the dictionary
        result.Add("checks", checks);

        // write the json
        httpContext.Response.ContentType = "application/health+json";
        httpContext.Response.StatusCode = healthReport.Status == HealthStatus.Unhealthy ? (int)System.Net.HttpStatusCode.ServiceUnavailable : (int)System.Net.HttpStatusCode.OK;
        return httpContext.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
    }

    /// <summary>
    /// Write the Health Check results as json
    /// </summary>
    /// <param name="httpContext">HttpContext</param>
    /// <param name="res">HealthCheckResult</param>
    /// <param name="serviceId">Service Id</param>
    /// <param name="duration">Duration</param>
    /// <returns>Task</returns>
    public static Task Writer(HttpContext httpContext, HealthCheckResult res, string serviceId, TimeSpan duration)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        // Convert the HealthCheckResult to a HealthReport
        HealthReport rpt = new (
            new Dictionary<string, HealthReportEntry> { { serviceId, new HealthReportEntry(res.Status, res.Description, duration, res.Exception, res.Data) } },
            duration);

        // call the response writer
        return Writer(httpContext, rpt, serviceId, res.Description);
    }

    /// <summary>
    /// Convert the dotnet HealthStatus to the IETF Status
    /// </summary>
    /// <param name="status">HealthStatus (dotnet)</param>
    /// <returns>string</returns>
    public static string ToIetfStatus(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Healthy => "pass",
            HealthStatus.Degraded => "warn",
            _ => "fail",
        };
    }
}
