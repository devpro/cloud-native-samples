using LogzioJaegerSample.Lib.DistributedTracing.Middleware;
using Microsoft.AspNetCore.Builder;

namespace LogzioJaegerSample.Lib.DistributedTracing.Builder
{
    public static class JaegerBuilderExtensions
    {
        public static IApplicationBuilder UseJaeger(this IApplicationBuilder app, IJaegerClientConfiguration jaegerClientConfiguration)
        {
            if (jaegerClientConfiguration.IsEnabled)
            {
                app.UseMiddleware<JaegerHttpMiddleware>();
            }

            return app;
        }
    }
}
