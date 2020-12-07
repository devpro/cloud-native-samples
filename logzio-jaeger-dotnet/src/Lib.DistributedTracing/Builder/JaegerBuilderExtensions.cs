using Microsoft.AspNetCore.Builder;

namespace LogzioJaegerSample.Lib.DistributedTracing.Builder
{
    public static class JaegerBuilderExtensions
    {
        public static IApplicationBuilder UseJaeger(this IApplicationBuilder app)
        {
            return app;
        }
    }
}
