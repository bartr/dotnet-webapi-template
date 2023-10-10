// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace Kic.Middleware;

public static class Logging
{
    // configure SeriLog
    public static void ConfigureSeriLog(LogEventLevel logLevel, string app, string version = "", string region = "", string zone = "", string node = "")
    {
        // configure SeriLog
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override(app, LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)

            // todo - are the warning benign?
            .MinimumLevel.Override("Microsoft.AspNetCore.OData.Routing", LogEventLevel.Error)

            // don't log these requests
            .Filter.ByExcluding(Matching.WithProperty<string>("HostingRequestStartingLog", v => !string.IsNullOrWhiteSpace(v)))
            .Filter.ByExcluding(Matching.WithProperty<string>("Path", v => v.StartsWith("/vendor/")))
            .Filter.ByExcluding(Matching.WithProperty<string>("Path", v => v.StartsWith("/css/")))
            .Filter.ByExcluding(Matching.WithProperty<string>("Path", v => v.StartsWith("/js/")))
            .Filter.ByExcluding(Matching.WithProperty<string>("Path", v => v.StartsWith("/img/")))
            .Filter.ByExcluding(Matching.WithProperty<string>("Path", v => v.StartsWith("/images/")))
            .Filter.ByExcluding(Matching.WithProperty<string>("Path", v => v.StartsWith("/favicon")))
            .Filter.ByExcluding(Matching.WithProperty<string>("QueryString", v => v.Contains("handler=List")))

            // add / remove properties
            .Enrich.With(new RemoveLogProperties())
            .Enrich.WithProperty("environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")

            // write to console in json format
            .WriteTo.Console(new ExpressionTemplate("{ { date: UtcDateTime(@t), level: @l, ..@p } }\n\n", theme: TemplateTheme.Code));

        // add optional properties
        if (!string.IsNullOrWhiteSpace(version))
        {
            loggerConfig.Enrich.WithProperty("version", version);
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            loggerConfig.Enrich.WithProperty("region", region);
        }

        if (!string.IsNullOrWhiteSpace(zone))
        {
            loggerConfig.Enrich.WithProperty("zone", zone);
        }

        if (!string.IsNullOrWhiteSpace(node))
        {
            loggerConfig.Enrich.WithProperty("node", node);
        }

        // set the static logger
        Log.Logger = loggerConfig.CreateLogger();
    }
}
