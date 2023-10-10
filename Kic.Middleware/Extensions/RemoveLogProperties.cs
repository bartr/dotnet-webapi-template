// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Serilog.Core;
using Serilog.Events;

namespace Kic.Middleware;

/// <summary>
/// Remove unused properties from request log
/// </summary>
public class RemoveLogProperties : ILogEventEnricher
{
    public void Enrich(LogEvent le, ILogEventPropertyFactory propertyFactory)
    {
        le.RemovePropertyIfPresent("HostingRequestFinishedLog");
        le.RemovePropertyIfPresent("RequestId");
        le.RemovePropertyIfPresent("ConnectionId");
        le.RemovePropertyIfPresent("PathBase");
    }
}
