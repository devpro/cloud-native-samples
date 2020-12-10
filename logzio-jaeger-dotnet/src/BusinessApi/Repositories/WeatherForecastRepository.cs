using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LogzioJaegerSample.DataApi.Dto;
using Microsoft.Extensions.Logging;
using Withywoods.Net.Http;

namespace LogzioJaegerSample.BusinessApi.Repositories
{
    public class WeatherForecastRepository : HttpRepositoryBase
    {
        public WeatherForecastRepository(ILogger<WeatherForecastRepository> logger, IHttpClientFactory httpClientFactory)
            : base(logger, httpClientFactory)
        {
        }

        protected override string HttpClientName => "DataApi";

        public async Task<List<WeatherForecastDto>> FindAllAsync()
        {
            return await GetAsync<List<WeatherForecastDto>>("WeatherForecast");
        }
    }
}
