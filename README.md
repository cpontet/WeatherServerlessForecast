# Blazor Starter Application

This template contains an example [Blazor WebAssembly](https://docs.microsoft.com/aspnet/core/blazor/?view=aspnetcore-3.1#blazor-webassembly) client application, a C# [Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-overview) and a C# class library with shared code.

## Getting Started

Create a repository from the [GitHub template](https://docs.github.com/en/enterprise/2.22/user/github/creating-cloning-and-archiving-repositories/creating-a-repository-from-a-template) and then clone it locally to your machine.

Once you clone the project, open the solution in [Visual Studio](https://visualstudio.microsoft.com/vs/community/) and follow these steps:

- Rename `local.settings.example.json` to `local.settings.json`
- Press **F5** to launch both the client application and the Functions API app

_Note: If you're using the Azure Functions CLI tools, refer to [the documentation](https://docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash) on how to enable CORS._

## Template Structure

* **Client**: The Blazor WebAssembly sample application
* **API**: A C# Azure Functions API, which the Blazor application will call
* **Shared**: A C# class library with a shared data model between the Blazor and Functions application

## Deploy to Azure Static Web Apps

This application can be deployed to [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps), to learn how, check out [our quickstart guide](https://aka.ms/blazor-swa/quickstart).



## Workshop

### Create an Azure CosmosDB

* Go to the Azure Portal
* Create a new CosmosDB instance
* Create a database called *WeatherForecast*
* Create a collection called *Data*

Manually add the following records through the *Data Explorer *panel

``` json
{
    "id": "1",
    "date": "2018-05-06",
    "temperatureC": 1,
    "summary": "Freezing"
}

{
    "id": "2",
    "date": "2018-05-07",
    "temperatureC": 14,
    "summary": "Bracing"
}

{
    "id": "3",
    "date": "2018-05-08",
    "temperatureC": -13,
    "summary": "Freezing"
}

{
    "id": "4",
    "date": "2018-05-09",
    "temperatureC": -16,
    "summary": "Balmy"
}

{
    "id": "5",
    "date": "2018-05-10",
    "temperatureC": -2,
    "summary": "Chilly"
}

```


### Add a CosmosDBTrigger function

* Create a function called CosmosDbFunctions with a ComsosDBTrigger.
* Configure connection string to point to your CosmosDB instance. Connection string can be found in the *Keys* pannel in the *Azure Portal*.
* Specify *WeatherForecast* as database name
* Specify *Data* as collection name


``` Csharp
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
```

For more info look at [Create a function triggered by Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-cosmos-db-triggered-function)

* Test the function
  * Start the Api project in Visual Studio
  * Add a new record in the database and see that the function is triggered

### Modify the WeatherForecast function to read from the database

* Open *WeatherForecastFunction.cs*
* Modify the signature and body of the function

``` Csharp
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
```

* Test the function
  * Start both the Api and Client projects in Visual Studio
  * Navigate to the *Fetch data* page in the web app
  * Notice that the data is coming from your database
 

### Add WeatherByDate function

* Add a new function called *WeatherByDate*
* Select *HttpTrigger*
* Modify the signature and body of the function
  * Notice the *Route* parameter in the CosmosDB binding attribute
  * Notice the SqlQuery parameter in teh CosmosDB binding attrebute

``` Csharp
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
```

### Generate weather data

* Add a function called *CollectWeatherData*
* Select *TimerTrigger*
* Specify time interval to 1 minute `00:01:00`
* Cut/Paste the *GetSummary* method from *WeatherForecast* function into *CollectWeatherData*
* Add a CosmosDB output binding to write data to the database

``` Csharp
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
```