namespace SplunkOpenTelemetrySample.WebApi.DependencyInjection
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, ApplicationConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(configuration.OpenApi.Version, configuration.OpenApi);
            });

            return services;
        }
    }
}
