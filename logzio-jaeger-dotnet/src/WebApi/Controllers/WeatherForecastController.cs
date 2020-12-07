using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILoggerFactory loggerFactory, ILogger<WeatherForecastController> logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
        }


        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // POC for traces

            Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new Jaeger.Senders.SenderResolver(_loggerFactory)
                .RegisterSenderFactory<Jaeger.Senders.Thrift.ThriftSenderFactory>();

            Jaeger.Configuration config = Jaeger.Configuration.FromEnv(_loggerFactory);

            var samplerConfiguration = new Jaeger.Configuration.SamplerConfiguration(_loggerFactory)
                .WithType(Jaeger.Samplers.ConstSampler.Type)
                .WithParam(1);

            var reporterConfiguration = new Jaeger.Configuration.ReporterConfiguration(_loggerFactory)
                .WithLogSpans(true);

            OpenTracing.ITracer tracer = config
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration)
                .GetTracer();

            OpenTracing.ISpanBuilder builder = tracer.BuildSpan("myop");

            OpenTracing.ISpan span = builder.Start();

            span.Log("toto");

            span.Finish();

            // End of POC

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
