using System;
using BlazorApp.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Api
{
    public static class CollectWeatherDataFunction
    {
        private static Random random = new Random();

        [FunctionName("CollectWeatherData")]
        public static void Run([TimerTrigger("00:01:00")]TimerInfo myTimer,
            [CosmosDB("WeatherForecast", "Data",
                ConnectionStringSetting = "ConnectionStrings:WeatherForecastDatabase"
                )] out WeatherForecast weatherForecat,
            ILogger log)
        {
            var temp = random.Next(-20, 55);
            weatherForecat = new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = temp,
                Summary = GetSummary(temp)
            };

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
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
