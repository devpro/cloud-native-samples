namespace SplunkOpenTelemetrySample.WebApi.Builder
{
    public static class TransportSecurityBuilderExtensions
    {
        public static IApplicationBuilder UseHttps(this IApplicationBuilder app, ApplicationConfiguration configuration)
        {
            if (configuration.IsHttpsRedirectionEnabled)
            {
                app.UseHttpsRedirection();
            }

            return app;
        }
    }
}
