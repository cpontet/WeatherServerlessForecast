using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace BlazorApp.Api
{
    public static class WeatherForecastFunction
    {
        [FunctionName("WeatherForecast")]
        public static IActionResult GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weatherforecast")] HttpRequest req,
            [CosmosDB("WeatherForecast", "Data",
                ConnectionStringSetting = "ConnectionStrings:WeatherForecastDatabase"                
                )] IEnumerable<WeatherForecast> weatherForecatItems,
            ILogger log)
        {
            log.LogInformation($"Fetched {weatherForecatItems.Count()} items from Weatherforecast");
            
            return new OkObjectResult(weatherForecatItems);
        }

        [FunctionName("OnWeatherChanged")]
        public static void OnWeatherChanged([CosmosDBTrigger("WeatherForecast", "Data",
                ConnectionStringSetting = "ConnectionStrings:WeatherForecastDatabase",
                LeaseCollectionName = "leases",
                CreateLeaseCollectionIfNotExists = true
            )]IReadOnlyList<Document> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                foreach (var item in input)
                {
                    WeatherForecast weatherforecast = JsonConvert.DeserializeObject<WeatherForecast>(item.ToString());
                    log.LogInformation("Date " + weatherforecast.Date);
                    log.LogInformation("Temp " + weatherforecast.TemperatureC);
                    log.LogInformation("Summary " + weatherforecast.Summary);
                }
            }
        }

        [FunctionName("WeatherByDate")]
        public static IActionResult ByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weatherforecast/{date}")] HttpRequest req,
            [CosmosDB("WeatherForecast", "Data",
                ConnectionStringSetting = "ConnectionStrings:WeatherForecastDatabase",
                SqlQuery = "select * from WeatherForecast w where w.Date = {date}")]
                IEnumerable<WeatherForecast> weatherforecastItems,
            ILogger log)
        {
            if (weatherforecastItems.Any())
                return new OkObjectResult(weatherforecastItems.First());
            else
                return new NotFoundResult();
        }

        [FunctionName("GenerateWeatherData")]
        public static IActionResult Generate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "weatherforecast/generate")] HttpRequest req,
            [CosmosDB("WeatherForecast", "Data",
                        ConnectionStringSetting = "ConnectionStrings:WeatherForecastDatabase"
                        )] out WeatherForecast weatherForecast,
            ILogger log)
        {
            Random random = new Random();
            var temp = random.Next(-20, 55);
            weatherForecast = new WeatherForecast
            {
                Date = DateTime.Now.ToUniversalTime(),
                TemperatureC = temp,
                Summary = GetSummary(temp)
            };

            log.LogInformation($"Generated new weatherforecast at {weatherForecast.Date}");

            var location = $"{req.Scheme}://{req.Host}/api/weatherforecast/{weatherForecast.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff'Z'")}";
            return new CreatedResult(location, weatherForecast);
        }

        private static string GetSummary(int temp)
        {
            var summary = "Mild";

            if (temp >= 32)
            {
                summary = "Hot";
            }
            else if (temp <= 16 && temp > 0)
            {
                summary = "Cold";
            }
            else if (temp <= 0)
            {
                summary = "Freezing";
            }

            return summary;
        }

    }
}
