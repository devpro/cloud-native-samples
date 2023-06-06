using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SplunkOpenTelemetrySample.WebApi.Dto;

namespace SplunkOpenTelemetrySample.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] s_summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private static readonly HttpClient s_httpClient = new();

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecastDto> Get()
        {
            var activity = Activity.Current;
            _logger.LogDebug($"Activity TraceId={activity.TraceId}, SpanId={activity.SpanId}, ParentSpanId={activity.ParentSpanId}");
            activity.AddEvent(new ActivityEvent("sample activity event."));

            using var scope = _logger.BeginScope("{Id}", Guid.NewGuid().ToString("N"));

            // Example of how dependency calls will be captured and treated automatically as child of incoming request
            var res = s_httpClient.GetStringAsync("http://google.com").Result;

            var rng = new Random();
            var forecast = Enumerable.Range(1, 5)
                .Select(index =>
                    new WeatherForecastDto
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = rng.Next(-20, 55),
                        Summary = s_summaries[rng.Next(s_summaries.Length)]
                    })
                .ToArray();

            _logger.LogInformation($"WeatherForecasts generated {forecast.Length}: {forecast}");

            return forecast;
        }
    }
}
