# Blazor Workshop

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


## Create an Azure CosmosDB

* Go to the Azure Portal
* Create a new CosmosDB instance
* Create a database called `WeatherForecast`
* Create a collection called `Data`

Manually add the following records through the *Data Explorer *panel

``` json
{
    "Date": "2018-05-06T17:00:00.0000000Z",
    "TemperatureC": 1,
    "Summary": "Freezing"
}

{
    "Date": "2018-05-07-06T15:00:00.0000000Z",
    "TemperatureC": 14,
    "Summary": "Bracing"
}

{
    "Date": "2018-05-08-06T08:00:00.0000000Z",
    "TemperatureC": -13,
    "Summary": "Freezing"
}

{
    "Date": "2018-05-09-06T10:00:00.0000000Z",
    "TemperatureC": -16,
    "Summary": "Balmy"
}

{
    "Date": "2018-05-10-06T11:00:00.0000000Z",
    "TemperatureC": -2,
    "Summary": "Chilly"
}

```

## Create a new branch

* Create a new branch called `CosmosDBBindings`


## Add a CosmosDBTrigger function

* Create a function called `OnWeatherChanged` with a `ComsosDBTrigger`.
* Configure connection string to point to your CosmosDB instance
  * The connection string can be found in the *Keys* pannel in the *Azure Portal*
  * You can configure it as a local secret
* Specify `WeatherForecast` as database name
* Specify `Data` as collection name
* Once the function has been generated by the wizard
  * Rename the function method to `OnWeatherChanged`
  * Make sure to add parameter `CreateLeaseCollectionIfNotExists` and set it to `true`
* Move this function to the file `WeatherForecastFunction.cs`
* Delete the file `OnWeatherChanged.cs`


``` Csharp
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
```

For more info look at [Create a function triggered by Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-cosmos-db-triggered-function)

* Test the function
  * Start the Api project in Visual Studio
  * Manully add a new record in the database and see that the function is triggered

## Modify the WeatherForecast function to read from the database

* Open `WeatherForecastFunction.cs`
* Rename the function method from `Run` to `GetAll`
* Add an `HttpTrigger` 
  * Set the verb to `get`
  * Set the `Route` parameter to `weatherforecast`
* Add a `CosmosDB` input binding
  * Set the `ConnectionStringSetting` parameter to your connection string settings value
  * Set the parameter to `IEnumerable<WeatherForecast> weatherForecatItems`

``` Csharp
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
```

* Test the function
  * Start both the Api and Client projects in Visual Studio
  * Navigate to the *Fetch data* page in the web app
  * Notice that the data is now coming from your database
 

## Add a function to get weather data by date

* Add a function called `WeatherByDate` 
* Add an `HttpTrigger` attribute 
  * Set the verb to `get`
  * Set the `Route` parameter to `weatherforecast/{date}`
* Add a `CosmosDB` input binding attribute
  * Set the `ConnectionStringSetting` parameter to your connection string settings value
  * Set the `SqlQuery` parameter to `select * from WeatherForecast w where w.Date = {date}`
  * Set the parameter to `IEnumerable<WeatherForecast> weatherForecatItems`

``` Csharp
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
```

* Test the function
  * Run the `Api` project
  * Copy the URL of the function from the console
  * Paste it in your browser
  * Copy the date of one of your records from the database
  * Go back to your browser and replace `{date}` by the value your just copied
  * Hit *Enter*
  * You should see your record in the browser


## Generate weather data

* Add a function called `GenerateWeatherData`
* Add an *HttpTrigger* attribute
  * Set the verb to`post`
  * Set the `Route` parameter to `weatherforecast/generate`
* Add a `CosmosDB` output binding attribute
  * Set the `ConnectionStringSetting` parameter to your connection string settings value
  * Set the output parameter to `out WeatherForecast weatherForecat`

``` Csharp
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
```

* Test the function
  * Run the `Api` project
  * Copy the URL of the function from the console
  * Open Postman
  * Create a new *POST* request
  * Use the URL you just copied
  * Send the request
  * Notice the result in the *Body* tab
  * Navigate to the *Headers* tab
  * Copy the URL from the *Location* header
  * Paste it in a new *GET* request
  * Notice the result int the *Body* tab

## Create a pull request

* Create a pull request 
* Go to GitHub actions `https://github.com/<yourname>/WeatherServerlessForecats/actions`
* Notice that the workflow was triggered
* Wait for the deployment to be over
* Navigate to your Azure Static Web App
* Open the *Functions* tab
* You should see your functions there
* Open the *Configuration* tab
* Select the `Staging` environment in the drop down list
* Add your connection string
* Go to the *Environements* tab
* Browse the *Staging* environment
