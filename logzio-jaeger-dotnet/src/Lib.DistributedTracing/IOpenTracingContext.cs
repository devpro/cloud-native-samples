namespace LogzioJaegerSample.Lib.DistributedTracing
{
    public interface IOpenTracingContext
    {
        OpenTracing.ITracer Tracer { get; }
    }
}
