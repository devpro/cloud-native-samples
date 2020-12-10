using LogzioJaegerSample.Lib.DistributedTracing.Configuration;
using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing
{
    public class DefaultOpenTracingContext : IOpenTracingContext
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly IDistributedTracingConfiguration _distributedTracingConfiguration;

        private readonly OpenTracing.ITracer _tracer;

        public DefaultOpenTracingContext(ILoggerFactory loggerFactory, IDistributedTracingConfiguration distributedTracingConfiguration)
        {
            _loggerFactory = loggerFactory;
            _distributedTracingConfiguration = distributedTracingConfiguration;

            _tracer = BuildTracer();
        }

        public OpenTracing.ITracer Tracer { get { return _tracer; } }

        private OpenTracing.ITracer BuildTracer()
        {
            if (!_distributedTracingConfiguration.IsEnabled)
            {
                return new Jaeger.Tracer.Builder(nameof(DefaultOpenTracingContext))
                    .WithReporter(new Jaeger.Reporters.NoopReporter())
                    .WithSampler(new Jaeger.Samplers.ConstSampler(false))
                    .Build();
            }

            var config = BuildJaegerConfiguration(_loggerFactory, _distributedTracingConfiguration.Jaeger);

            Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new Jaeger.Senders.SenderResolver(_loggerFactory)
                .RegisterSenderFactory<Jaeger.Senders.Thrift.ThriftSenderFactory>();

            return config.GetTracer();
        }


        private Jaeger.Configuration BuildJaegerConfiguration(ILoggerFactory loggerFactory, JaegerConfiguration jaegerConfiguration)
        {
            var samplerConfiguration = new Jaeger.Configuration.SamplerConfiguration(loggerFactory)
                .WithType(Jaeger.Samplers.ConstSampler.Type)
                .WithParam(1);

            var senderConfiguration = new Jaeger.Configuration.SenderConfiguration(loggerFactory)
                .WithAgentHost(jaegerConfiguration.AgentHost)
                .WithAgentPort(jaegerConfiguration.AgentPort);

            var reporterConfiguration = new Jaeger.Configuration.ReporterConfiguration(loggerFactory)
                .WithLogSpans(true)
                .WithSender(senderConfiguration);

            var config = new Jaeger.Configuration(jaegerConfiguration.ServiceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration);

            return config;
        }
    }
}
