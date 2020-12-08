using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing
{
    public class DefaultOpenTracingContext : IOpenTracingContext
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly IJaegerClientConfiguration _clientConfiguration;

        private readonly OpenTracing.ITracer _tracer;

        public DefaultOpenTracingContext(ILoggerFactory loggerFactory, IJaegerClientConfiguration clientConfiguration)
        {
            _loggerFactory = loggerFactory;
            _clientConfiguration = clientConfiguration;

            _tracer = BuildTracer();
        }

        public OpenTracing.ITracer Tracer { get { return _tracer; } }

        private OpenTracing.ITracer BuildTracer()
        {
            if (!_clientConfiguration.IsEnabled)
            {
                return new Jaeger.Tracer.Builder(nameof(DefaultOpenTracingContext))
                    .WithReporter(new Jaeger.Reporters.NoopReporter())
                    .WithSampler(new Jaeger.Samplers.ConstSampler(false))
                    .Build();
            }

            var config = BuildJaegerConfiguration(_loggerFactory, _clientConfiguration);

            Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new Jaeger.Senders.SenderResolver(_loggerFactory)
                .RegisterSenderFactory<Jaeger.Senders.Thrift.ThriftSenderFactory>();

            return config.GetTracer();
        }


        private Jaeger.Configuration BuildJaegerConfiguration(ILoggerFactory loggerFactory, IJaegerClientConfiguration clientConfiguration)
        {
            var samplerConfiguration = new Jaeger.Configuration.SamplerConfiguration(loggerFactory)
                .WithType(Jaeger.Samplers.ConstSampler.Type)
                .WithParam(1);

            var senderConfiguration = new Jaeger.Configuration.SenderConfiguration(loggerFactory)
                .WithAgentHost(clientConfiguration.AgentHost)
                .WithAgentPort(clientConfiguration.AgentPort);

            var reporterConfiguration = new Jaeger.Configuration.ReporterConfiguration(loggerFactory)
                .WithLogSpans(true)
                .WithSender(senderConfiguration);

            var config = new Jaeger.Configuration(clientConfiguration.ServiceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration);

            return config;
        }
    }
}
