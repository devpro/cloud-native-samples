using System.Net.Http.Headers;
using LogzioJaegerSample.Lib.DistributedTracing.Configuration;
using LogzioJaegerSample.Lib.DistributedTracing.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace LogzioJaegerSample.DataApi
{
    public class Startup
    {
        private const string DistributedTracingConfigurationKey = "DistributedTracing";

        private const string ApplicationName = "Data API";

        private const string ApplicationVersion = "v1";

        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApplicationVersion, new OpenApiInfo { Title = ApplicationName, Version = ApplicationVersion });
            });

            services.AddDistributedTracing(DistributedTracingConfiguration.Create(_configuration, DistributedTracingConfigurationKey));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{ApplicationVersion}/swagger.json", $"{ApplicationName} {ApplicationVersion}"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
