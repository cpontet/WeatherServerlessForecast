using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;
using System.Collections.Generic;

namespace BlazorApp.Api
{
    public static class WeatherForecastFunction
    {
        [FunctionName("WeatherForecast")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB("WeatherForecast", "Data",
                ConnectionStringSetting = "ConnectionStrings:WeatherForecastDatabase"                
                )] IEnumerable<WeatherForecast> weatherForecatItems,
            ILogger log)
        {
            log.LogInformation($"Fetched {weatherForecatItems.Count()} items from Weatherforecast");
            
            return new OkObjectResult(weatherForecatItems);
        }


    }
}
