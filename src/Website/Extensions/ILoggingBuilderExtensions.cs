﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Serilog;

namespace MartinCostello.Website.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="ILoggingBuilder"/> interface. This class cannot be inherited.
/// </summary>
public static class ILoggingBuilderExtensions
{
    /// <summary>
    /// Configures logging for the application.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
    /// <param name="context">The <see cref="HostBuilderContext"/> to use.</param>
    /// <returns>
    /// The <see cref="ILoggingBuilder"/> passed as the value of <paramref name="builder"/>.
    /// </returns>
    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder builder, HostBuilderContext context)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("AspNetCoreEnvironment", context.HostingEnvironment.EnvironmentName)
            .Enrich.WithProperty("AzureDatacenter", context.Configuration.AzureDatacenter())
            .Enrich.WithProperty("AzureEnvironment", context.Configuration.AzureEnvironment())
            .Enrich.WithProperty("Version", GitMetadata.Commit)
            .ReadFrom.Configuration(context.Configuration);

        string appInsightsConnectionString = context.Configuration.ApplicationInsightsConnectionString();

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            loggerConfig = loggerConfig.WriteTo.ApplicationInsights(appInsightsConnectionString, TelemetryConverter.Events);
        }

        if (context.HostingEnvironment.IsDevelopment())
        {
            loggerConfig = loggerConfig.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
        }

        Log.Logger = loggerConfig.CreateLogger();
        return builder.AddSerilog(dispose: true);
    }
}
