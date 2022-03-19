// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace CseLabs.Middleware
{
    /// <summary>
    /// cse-labs Logger Extensions
    /// </summary>
    public static class CseLoggerExtensions
    {
        public static ILoggingBuilder AddNgsaLogger(this ILoggingBuilder builder)
        {
            return builder.AddNgsaLogger(new CseLoggerConfiguration());
        }

        public static ILoggingBuilder AddNgsaLogger(this ILoggingBuilder builder, Action<CseLoggerConfiguration> configure)
        {
            CseLoggerConfiguration config = new ();
            configure(config);

            return builder.AddNgsaLogger(config);
        }

        public static ILoggingBuilder AddNgsaLogger(this ILoggingBuilder builder, CseLoggerConfiguration config)
        {
            return builder.AddProvider(new CseLoggerProvider(config));
        }
    }
}
