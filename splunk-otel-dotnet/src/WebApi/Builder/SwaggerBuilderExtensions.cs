namespace SplunkOpenTelemetrySample.WebApi.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, ApplicationConfiguration configuration)
        {
            if (configuration.IsSwaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{configuration.OpenApi.Version}/swagger.json",
                    $"{configuration.OpenApi.Title} {configuration.OpenApi.Version}"));
            }

            return app;
        }
    }
}
