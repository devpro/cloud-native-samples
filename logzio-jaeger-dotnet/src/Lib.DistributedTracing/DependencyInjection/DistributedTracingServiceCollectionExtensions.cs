using System;
using System.Linq;
using LogzioJaegerSample.Lib.DistributedTracing.Configuration;
using LogzioJaegerSample.Lib.DistributedTracing.Diagnostics;
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

            // OpenTelemetry
            if (configuration.IsEnabled
                && configuration.Framework == DistributedTracingFramework.OpenTelemetry
                && configuration.Reporter == DistributedTracingReporter.Jaeger)
            {
                services.AddOpenTelemetryTracing((builder) => builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(configuration.ServiceName))
                    .AddAspNetCoreInstrumentation(options =>
                        options.Filter = (httpContext) =>
                        {
                            if (string.IsNullOrEmpty(configuration.PathToIgnore))
                            {
                                return true;
                            }

                            foreach (var path in configuration.PathToIgnore.Split(','))
                            {
                                if (httpContext.Request.Path.StartsWithSegments(path))
                                {
                                    return false;
                                }
                            }

                            return true;
                        })
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(jaegerOptions =>
                    {
                        jaegerOptions.AgentHost = configuration.Jaeger.AgentHost;
                        jaegerOptions.AgentPort = configuration.Jaeger.AgentPort;
                    }));
            }

            // Diagnostics
            services.AddTransient(typeof(IActivityEventLogger<>), typeof(ActivityEventLogger<>));

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
