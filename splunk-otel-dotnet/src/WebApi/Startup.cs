using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SplunkOpenTelemetrySample.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(_configuration["Application:Version"],
                    new OpenApiInfo { Title = _configuration["Application:Name"], Version = _configuration["Application:Version"] });
            });

            if (bool.TryParse(_configuration["OpenTelemetryTracing:Enabled"], out var isOtelTracingEnabled) && isOtelTracingEnabled)
            {
                // example: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Startup.cs

                // Adding the OtlpExporter creates a GrpcChannel.
                // This switch must be set before creating a GrpcChannel/HttpClient when calling an insecure gRPC service.
                // See: https://docs.microsoft.com/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                services.AddOpenTelemetryTracing((builder) => builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(_configuration["OpenTelemetryTracing:Service"]))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(_configuration["OpenTelemetryTracing:OtlpExporter:Endpoint"]);
                    }));
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{_configuration["Application:Version"]}/swagger.json",
                    $"{_configuration["Application:Name"]} {_configuration["Application:Version"]}"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
