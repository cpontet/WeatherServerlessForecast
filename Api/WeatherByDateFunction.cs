using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BlazorApp.Shared;
using System.Collections.Generic;

namespace BlazorApp.Api
{
    public static class WeatherByDateFunction
    {
        [FunctionName("WeatherByDate")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weatherbydate/{date}")] HttpRequest req,
            [CosmosDB("WeatherForecast", "Data",
                ConnectionStringSetting = "ConnectionStrings:WeatherForecastDatabase",
                SqlQuery = "select * from WeatherForecast w where w.date = {date}")]
                IEnumerable<WeatherForecast> weatherforecastItems,
            ILogger log)
        {
            log.LogInformation($"Found {weatherforecastItems.Count()} weather forecast items matching this date");

            return new OkObjectResult(weatherforecastItems);
        }
    }
}
