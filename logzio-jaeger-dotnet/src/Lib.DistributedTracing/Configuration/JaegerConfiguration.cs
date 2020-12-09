namespace LogzioJaegerSample.Lib.DistributedTracing.Configuration
{
    public class JaegerConfiguration
    {
        public string ServiceName { get; set; }

        public string AgentHost { get; set; }

        public int AgentPort { get; set; } = 6831;
    }
}
