using LogzioJaegerSample.Lib.DistributedTracing.Configuration;
using LogzioJaegerSample.Lib.DistributedTracing.Middleware;
using Microsoft.AspNetCore.Builder;

namespace LogzioJaegerSample.Lib.DistributedTracing.Builder
{
    public static class JaegerBuilderExtensions
    {
        public static IApplicationBuilder UseJaeger(this IApplicationBuilder app, IDistributedTracingConfiguration distributedTracingConfiguration)
        {
            if (distributedTracingConfiguration.IsEnabled)
            {
                app.UseMiddleware<OpenTracingHttpMiddleware>();
            }

            return app;
        }
    }
}
