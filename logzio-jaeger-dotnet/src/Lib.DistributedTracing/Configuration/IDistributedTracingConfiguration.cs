using OpenTelemetry.Exporter.Jaeger;

namespace LogzioJaegerSample.Lib.DistributedTracing.Configuration
{
    public enum DistributedTracingFramework
    {
        OpenTelemetry,
        OpenTracing
    }
    public enum DistributedTracingReporter
    {
        Jaeger
    }

    public interface IDistributedTracingConfiguration
    {
        bool IsEnabled { get; }

        string ServiceName { get; }

        DistributedTracingFramework Framework { get; }

        DistributedTracingReporter Reporter { get; }

        string PathToIgnore { get; }

        JaegerExporterOptions Jaeger { get; }
    }
}
