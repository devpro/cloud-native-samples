using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing
{
    public class OpenTracingContext : IOpenTracingContext
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly IJaegerClientConfiguration _clientConfiguration;

        private readonly OpenTracing.ITracer _tracer;

        public OpenTracingContext(ILoggerFactory loggerFactory, IJaegerClientConfiguration clientConfiguration)
        {
            _loggerFactory = loggerFactory;
            _clientConfiguration = clientConfiguration;

            _tracer = BuildTracer();
        }

        public OpenTracing.ITracer Tracer { get { return _tracer;  } }

        private OpenTracing.ITracer BuildTracer()
        {
            var config = _clientConfiguration.BuildJaegerConfiguration(_loggerFactory);

            Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new Jaeger.Senders.SenderResolver(_loggerFactory)
                .RegisterSenderFactory<Jaeger.Senders.Thrift.ThriftSenderFactory>();

            return config.GetTracer();
        }
    }
}
