using System.Collections.Generic;
using System.Threading.Tasks;
using LogzioJaegerSample.BusinessApi.Repositories;
using LogzioJaegerSample.DataApi.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.BusinessApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetaWeatherForecastController : ControllerBase
    {
        private readonly ILogger<MetaWeatherForecastController> _logger;

        private readonly WeatherForecastRepository _weatherForecastRepository;

        public MetaWeatherForecastController(ILogger<MetaWeatherForecastController> logger, WeatherForecastRepository weatherForecastRepository)
        {
            _logger = logger;
            _weatherForecastRepository = weatherForecastRepository;
        }

        [HttpGet]
        public async Task<List<WeatherForecastDto>> Get()
        {
            return await _weatherForecastRepository.FindAllAsync();
        }
    }
}
