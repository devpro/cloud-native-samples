using Microsoft.Extensions.Configuration;
using OpenTelemetry.Exporter.Jaeger;

namespace LogzioJaegerSample.Lib.DistributedTracing.Configuration
{
    public class DistributedTracingConfiguration : IDistributedTracingConfiguration
    {
        public bool IsEnabled { get; set; } = false;

        public string ServiceName { get; set; }

        public DistributedTracingFramework Framework { get; set; } = DistributedTracingFramework.OpenTelemetry;

        public DistributedTracingReporter Reporter { get; set; } = DistributedTracingReporter.Jaeger;

        public JaegerExporterOptions Jaeger { get; set; }

        public static IDistributedTracingConfiguration Create(IConfiguration configuration, string sectionName)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(sectionName))
            {
                throw new System.ArgumentException($"'{nameof(sectionName)}' cannot be null or empty", nameof(sectionName));
            }

            var model = new DistributedTracingConfiguration();
            configuration.GetSection(sectionName).Bind(model);
            return model;
        }
    }
}
