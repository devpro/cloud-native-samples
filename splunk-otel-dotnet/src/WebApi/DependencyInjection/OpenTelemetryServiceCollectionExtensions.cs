﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SplunkOpenTelemetrySample.WebApi.DependencyInjection
{
    public static class OpenTelemetryServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, ApplicationConfiguration configuration, ILoggingBuilder logging)
        {
            if (!configuration.IsOpenTelemetryEnabled)
            {
                return services;
            }

            if (logging == null)
            {
                throw new ArgumentNullException(nameof(logging));
            }

            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                    tracerProviderBuilder
                        .AddSource(configuration.OpenTelemetryService)
                        .ConfigureResource(resourceBuilder => resourceBuilder
                            .AddService(configuration.OpenTelemetryService))
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = (httpContext) =>
                            {
                                var pathsToIgnore = "/health,/favicon.ico";
                                return !pathsToIgnore.Split(',').Any(path => httpContext.Request.Path.StartsWithSegments(path));
                            };
                        })
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options => options.Endpoint = new Uri(configuration.OpenTelemetryCollectorEndpoint)))
                .WithMetrics(metricsProviderBuilder =>
                    metricsProviderBuilder
                        .ConfigureResource(resourceBuilder => resourceBuilder
                            .AddService(configuration.OpenTelemetryService))
                        .AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options => options.Endpoint = new Uri(configuration.OpenTelemetryCollectorEndpoint)));

            // logs
            logging.AddOpenTelemetry(loggerOptions =>
            {
                loggerOptions.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration.OpenTelemetryService));
                loggerOptions.IncludeFormattedMessage = true;
                loggerOptions.IncludeScopes = true;
                loggerOptions.ParseStateValues = true;
                loggerOptions.AddOtlpExporter(options => options.Endpoint = new Uri(configuration.OpenTelemetryCollectorEndpoint));
            });

            return services;
        }
    }
}
