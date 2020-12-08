using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogzioJaegerSample.Lib.DistributedTracing.DependencyInjection
{
    public static class JaegerServiceCollectionExtensions
    {
        public static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration configuration, string sectionName)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentException($"'{nameof(sectionName)}' cannot be null or empty", nameof(sectionName));
            }

            // configuration
            IJaegerClientConfiguration model = new JaegerClientConfiguration();
            configuration.GetSection(sectionName).Bind(model);
            services.AddSingleton(x => { return model; });

            // tracer
            services.AddScoped<IOpenTracingContext, DefaultOpenTracingContext>();

            return services;
        }
    }
}
