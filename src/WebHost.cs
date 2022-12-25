// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using MC.Middleware;
using Prometheus;
using Serilog;
using Serilog.Templates;

namespace CSApp;

/// <summary>
/// Main application class
/// </summary>
public partial class Program
{
    // configure Prometheus metrics
    private static void ConfigPrometheus(WebApplication app)
    {
        var requestHistogram = Metrics.CreateHistogram(
                    "CSAppDuration",
                    "Histogram of app request duration",
                    new HistogramConfiguration
                    {
                        Buckets = Histogram.ExponentialBuckets(1, 2, 10),
                        LabelNames = new string[] { "code", "mode", "region", "zone" },
                    });

        var requestSummary = Metrics.CreateSummary(
            "CSAppSummary",
            "Summary of app request duration",
            new SummaryConfiguration
            {
                SuppressInitialValue = true,
                MaxAge = TimeSpan.FromMinutes(5),
                Objectives = new List<QuantileEpsilonPair> { new QuantileEpsilonPair(.9, .0), new QuantileEpsilonPair(.95, .0), new QuantileEpsilonPair(.99, .0), new QuantileEpsilonPair(1.0, .0) },
                LabelNames = new string[] { "code", "mode", "region", "zone" },
            });

        app.Use(async Task (context, next) =>
        {
            DateTime dtStart = DateTime.Now;
            double duration = 0;

            // Invoke next handler
            if (next != null)
            {
                await next().ConfigureAwait(false);
            }

            await context.Response.CompleteAsync();

            string mode = context.Request.Path.StartsWithSegments("/api") ? "api" : "static";

            // compute request duration
            duration = Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds, 2);
            Console.WriteLine(duration);
            requestHistogram.WithLabels(GetPrometheusCode(context.Response.StatusCode), mode, Config.Region, Config.Zone).Observe(duration);
            requestSummary.WithLabels(GetPrometheusCode(context.Response.StatusCode), mode, Config.Region, Config.Zone).Observe(duration);
        });

        //var cpuGauge = Metrics.CreateGauge(
        //    "CSAppCpuPercent",
        //    "CPU Percent Used",
        //    new GaugeConfiguration
        //    {
        //        SuppressInitialValue = true,
        //        LabelNames = new string[] { "code", "mode", "region", "zone" },
        //    });

        app.UseMetricServer("/metrics");
    }

    // configure the WebApplication
    private static void ConfigApp(WebApplication app)
    {
        app.UseSerilogRequestLogging();

        ConfigPrometheus(app);

        // MC middleware
        app.UseVersion()
            .UseHealthz()
            .UseReadyz()
            .UseResetData();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
        }

        // UseHsts in prod
        if (app.Environment.IsProduction())
        {
            app.UseHsts();
        }

        // redirect /
        app.Use(async (context, next) =>
        {
            // matches /
            if (context.Request.Path.Equals("/") || context.Request.Path.Equals("/"))
            {
                // return the version info
                context.Response.Redirect($"/index.html", true);
                return;
            }
            else
            {
                // not a match, so call next middleware handler
                await next().ConfigureAwait(false);
            }
        });

        // add middleware handlers
        app.UseRouting()

            //.UseEndpoints(ep =>
            //{
            //    ep.MapControllers();
            //})
            .UseStaticFiles()
            .UseCors()
            .UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/v1/openapi.json", "Iventory Service (mock)");
                c.RoutePrefix = string.Empty;
            });

        app.MapControllers();
    }

    // build the WebApplication host
    private static WebApplication BuildHost()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseUrls($"http://*:8080/");
        builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(10));

        builder.Logging.ClearProviders().AddSerilog();

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Serilog.AspNetCore.RequestLoggingMiddleware", Config.RequestLogLevel)
            .MinimumLevel.Override("Microsoft", Config.LogLevel)
            .WriteTo.Console(new ExpressionTemplate("{ {@t, @l, @x, ..@p} }\n")));

        //.WriteTo.Console(new RenderedCompactJsonFormatter()))

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // add swagger
        builder.Services.AddSwaggerGen();

        // add cors
        builder.Services.AddCors(o =>
        {
            o.AddPolicy(
                name: "all",
                policy =>
                {
                    policy.AllowAnyOrigin();
                });
        });

        return builder.Build();
    }

    // convert status code to string group for Prom metrics
    private static string GetPrometheusCode(int statusCode)
    {
        if (statusCode >= 500)
        {
            return "Error";
        }
        else if (statusCode == 429)
        {
            return "Retry";
        }
        else if (statusCode >= 400)
        {
            return "Warn";
        }
        else
        {
            return "OK";
        }
    }
}
