using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing
{
    public class DefaultJaegerClientConfiguration : IJaegerClientConfiguration
    {
        // TODO: use Jaeger.Reporters.NoopReporter if IsEnabled is false

        public bool IsEnabled { get; set; } = false;

        public string ServiceName { get; set; }

        public string AgentHost { get; set; }

        public int AgentPort { get; set; } = 6831;

        public Jaeger.Configuration BuildJaegerConfiguration(ILoggerFactory loggerFactory)
        {
            var samplerConfiguration = new Jaeger.Configuration.SamplerConfiguration(loggerFactory)
                .WithType(Jaeger.Samplers.ConstSampler.Type)
                .WithParam(1);

            var senderConfiguration = new Jaeger.Configuration.SenderConfiguration(loggerFactory)
                .WithAgentHost(AgentHost)
                .WithAgentPort(AgentPort);

            var reporterConfiguration = new Jaeger.Configuration.ReporterConfiguration(loggerFactory)
                .WithLogSpans(true)
                .WithSender(senderConfiguration);

            var config = new Jaeger.Configuration(ServiceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration);
            
            return config;
        }
    }
}
