// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Serilog.Events;

namespace CSApp
{
    public class Config
    {
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Warning;
        public bool IsLogLevelSet { get; set; }
        public bool DryRun { get; set; }
        public int CacheDuration { get; set; } = 300;
        public string Zone { get; set; }
        public string Region { get; set; }
        public int Port { get; set; } = 8080;
        public int Retries { get; set; } = 10;
        public int Timeout { get; set; } = 10;
        public LogEventLevel RequestLogLevel { get; set; } = LogEventLevel.Information;

        public string DataFilePath { get; set; } = Path.Combine("data", "data.json");
        public string SwaggerFilePath { get; set; } = Path.Combine("cadl", "sampleData.json");
        public string SwaggerUri { get; set; } = "/v1/openapi.json";
        public string Title { get; set; } = "CSApp";
        public string Namespace { get; } = "CSApp";
        public string Service { get; } = "CSApp";
        public string Version { get; } = GetVersion();
        public string ApiPathBase { get; } = "/api/v1";

        public void SetConfig(Config config)
        {
            IsLogLevelSet = config.IsLogLevelSet;
            DryRun = config.DryRun;
            CacheDuration = config.CacheDuration;
            Port = config.Port;
            Retries = config.Retries;
            Timeout = config.Timeout;

            // LogLevel.Information is the min
            LogLevel = config.LogLevel <= LogEventLevel.Information ? LogEventLevel.Information : config.LogLevel;
            RequestLogLevel = config.RequestLogLevel <= LogEventLevel.Information ? LogEventLevel.Information : config.RequestLogLevel;

            // clean up string values
            Zone = string.IsNullOrWhiteSpace(config.Zone) ? string.Empty : config.Zone.Trim();
            Region = string.IsNullOrWhiteSpace(config.Region) ? string.Empty : config.Region.Trim();
        }

        // get the version info
        private static string GetVersion()
        {
            // use reflection
            if (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
            {
                return v.InformationalVersion;
            }

            return "unknown";
        }
    }
}
