using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LogzioJaegerSample.BusinessApi.Repositories;
using LogzioJaegerSample.DataApi.Dto;
using LogzioJaegerSample.Lib.DistributedTracing.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LogzioJaegerSample.BusinessApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetaWeatherForecastController : ControllerBase
    {
        private readonly IActivityEventLogger<MetaWeatherForecastController> _logger;

        private readonly WeatherForecastRepository _weatherForecastRepository;

        public MetaWeatherForecastController(IActivityEventLogger<MetaWeatherForecastController> logger, WeatherForecastRepository weatherForecastRepository)
        {
            _logger = logger;
            _weatherForecastRepository = weatherForecastRepository;
        }

        [HttpGet]
        public async Task<List<WeatherForecastDto>> Get()
        {
            _logger.LogInformation("Processing business request to fetch all WeatherForecast");

            _logger.LogDebug("Dummy message that should not be seen");

            #region POC

            //var activity = Activity.Current;
            //_logger.LogDebug($"Activity TraceId={activity.TraceId}, SpanId={activity.SpanId}, ParentSpanId={activity.ParentSpanId}");
            //activity.AddEvent(new ActivityEvent("sample activity event."));

            #endregion

            return await _weatherForecastRepository.FindAllAsync();
        }
    }
}
