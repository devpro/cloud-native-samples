﻿using System;
using System.Collections.Generic;
using System.Linq;
using LogzioJaegerSample.DataApi.Dto;
using LogzioJaegerSample.Lib.DistributedTracing;
using LogzioJaegerSample.Lib.DistributedTracing.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.DataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IActivityEventLogger<WeatherForecastController> _logger;

        private readonly OpenTracing.ITracer _jaegerTracer;

        public WeatherForecastController(IActivityEventLogger<WeatherForecastController> logger/*, IOpenTracingContext openTracingContext*/)
        {
            _logger = logger;
            //_jaegerTracer = openTracingContext.Tracer;
        }

        [HttpGet]
        public IEnumerable<WeatherForecastDto> Get()
        {
            #region POC

            //OpenTracing.ISpanBuilder builder = _jaegerTracer.BuildSpan("myop");

            //OpenTracing.ISpan span = builder.Start();

            //span.Log("toto");

            //span.Finish();

            #endregion

            _logger.LogInformation("Generating random WeatherForecast data");

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecastDto
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
