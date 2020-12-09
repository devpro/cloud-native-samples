using System;
using LogzioJaegerSample.Lib.DistributedTracing.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LogzioJaegerSample.Lib.DistributedTracing.DependencyInjection
{
    public static class DistributedTracingServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedTracing(this IServiceCollection services, IDistributedTracingConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // configuration
            services.AddSingleton(x => { return configuration; });

            // telemetry
            if (configuration.IsEnabled
                && configuration.Framework == DistributedTracingFramework.OpenTelemetry
                && configuration.Reporter == DistributedTracingReporter.Jaeger)
            {
                services.AddOpenTelemetryTracing((builder) => builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration.Jaeger.ServiceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(jaegerOptions =>
                    {
                        jaegerOptions.AgentHost = configuration.Jaeger.AgentHost;
                        jaegerOptions.AgentPort = configuration.Jaeger.AgentPort;
                    }));
            }

            // TODO: review tracer
            services.AddScoped<IOpenTracingContext, DefaultOpenTracingContext>();

            return services;
        }
    }
}
