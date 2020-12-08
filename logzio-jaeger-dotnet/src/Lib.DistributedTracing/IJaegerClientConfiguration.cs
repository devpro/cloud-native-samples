using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing
{
    public interface IJaegerClientConfiguration
    {
        bool IsEnabled { get; }
        string ServiceName { get; }
        string AgentHost { get; }
        int AgentPort { get; }
    }
}
