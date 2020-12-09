using Microsoft.Extensions.Configuration;

namespace LogzioJaegerSample.Lib.DistributedTracing.Configuration
{
    public class DistributedTracingConfiguration : IDistributedTracingConfiguration
    {
        public bool IsEnabled { get; set; } = false;

        public DistributedTracingFramework Framework { get; set; } = DistributedTracingFramework.OpenTelemetry;

        public DistributedTracingReporter Reporter { get; set; } = DistributedTracingReporter.Jaeger;

        public JaegerConfiguration Jaeger { get; set; }

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
