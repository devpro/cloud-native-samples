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

        DistributedTracingFramework Framework { get; }

        DistributedTracingReporter Reporter { get; }

        JaegerConfiguration Jaeger { get; }
    }
}
