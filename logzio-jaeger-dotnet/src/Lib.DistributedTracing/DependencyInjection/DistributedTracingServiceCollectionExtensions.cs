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
            
            // telemetry
            if (configuration.IsEnabled
                && configuration.Framework == DistributedTracingFramework.OpenTelemetry
                && configuration.Reporter == DistributedTracingReporter.Jaeger)
            {
                services.AddOpenTelemetryTracing((builder) => builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration.ServiceName))
                    .AddAspNetCoreInstrumentation(options =>
                        options.Filter = (httpContext) =>
                        {
                            // do not trace calls to Swagger
                            return !httpContext.Request.Path.StartsWithSegments("/swagger");
                        })
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(jaegerOptions =>
                    {
                        jaegerOptions.AgentHost = configuration.Jaeger.AgentHost;
                        jaegerOptions.AgentPort = configuration.Jaeger.AgentPort;
                    }));
            }

            #region POC OpenTracing

            // TODO: review configuration
            //services.AddSingleton(x => { return configuration; });

            // TODO: review tracer
            //services.AddScoped<IOpenTracingContext, DefaultOpenTracingContext>();

            #endregion

            return services;
        }
    }
}
