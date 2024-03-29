﻿using Microsoft.OpenApi.Models;

namespace SplunkOpenTelemetrySample.WebApi
{
    public class ApplicationConfiguration
    {
        protected IConfigurationRoot ConfigurationRoot { get; }

        public ApplicationConfiguration(IConfigurationRoot configurationRoot)
        {
            ConfigurationRoot = configurationRoot;
        }

        // flags

        public bool IsOpenTelemetryEnabled => TryGetSection("Application:IsOpenTelemetryEnabled").Get<bool>();

        public bool IsHttpsRedirectionEnabled => TryGetSection("Application:IsHttpsRedirectionEnabled").Get<bool>();

        public bool IsSwaggerEnabled => TryGetSection("Application:IsSwaggerEnabled").Get<bool>();

        public bool IsCertificateValidationSkipped => TryGetSection("Application:IsCertificateValidationSkipped").Get<bool>();

        // definitions

        public static string CorsPolicyName => "RestrictedOrigins";

        public static string HealthCheckEndpoint => "/health";

        public OpenApiInfo OpenApi => TryGetSection("OpenApi").Get<OpenApiInfo>() ?? throw new Exception("");

        public string OpenTelemetryService => TryGetSection("OpenTelemetry:ServiceName").Get<string>() ?? "";

        // infrastructure

        public List<string> CorsAllowedOrigin => TryGetSection("AllowedOrigins").Get<List<string>>() ?? new List<string>();

        public string OpenTelemetryCollectorEndpoint => TryGetSection("OpenTelemetry:CollectorEndpoint").Get<string>() ?? "";

        // protected methods

        protected IConfigurationSection TryGetSection(string sectionKey)
        {
            return ConfigurationRoot.GetSection(sectionKey)
                ?? throw new ArgumentException("Missing section \"" + sectionKey + "\" in configuration", nameof(sectionKey));
        }
    }
}
