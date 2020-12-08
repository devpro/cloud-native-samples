namespace LogzioJaegerSample.Lib.DistributedTracing
{
    public class JaegerClientConfiguration : IJaegerClientConfiguration
    {
        public bool IsEnabled { get; set; } = false;

        public string ServiceName { get; set; }

        public string AgentHost { get; set; }

        public int AgentPort { get; set; } = 6831;
    }
}
