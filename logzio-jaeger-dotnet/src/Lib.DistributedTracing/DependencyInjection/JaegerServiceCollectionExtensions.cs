using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing.DependencyInjection
{
    public static class JaegerServiceCollectionExtensions
    {
        public static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration configuration, string sectionName)
        {
            // configuration
            IJaegerClientConfiguration model = new DefaultJaegerClientConfiguration();
            configuration.GetSection(sectionName).Bind(model);
            services.AddSingleton(x => { return model; });

            // tracer
            services.AddScoped<IOpenTracingContext, OpenTracingContext>();

            return services;
        }
    }
}
