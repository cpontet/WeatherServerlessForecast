using System;
using System.Collections.Generic;
using BlazorApp.Shared;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorApp.Api
{
    public static class CosmosDbFunctions
    {
        [FunctionName("CosmosDbFunctions")]
        public static void Run([CosmosDBTrigger(
                databaseName: "WeatherForecast",
                collectionName: "Data",
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
    }
}
