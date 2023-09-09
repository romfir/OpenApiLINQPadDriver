# OpenApiLINQPadDriver for LINQPad 7
[![Latest build](https://github.com/romfir/OpenApiLINQPadDriver/workflows/Build/badge.svg)](https://github.com/romfir/OpenApiLINQPadDriver/actions)
[![NuGet](https://img.shields.io/nuget/v/OpenApiLINQPadDriver)](https://www.nuget.org/packages/OpenApiLINQPadDriver)
[![Downloads](https://img.shields.io/nuget/dt/OpenApiLINQPadDriver)](https://www.nuget.org/packages/OpenApiLINQPadDriver)
[![License](https://img.shields.io/badge/license-MIT-yellow)](https://opensource.org/licenses/MIT)

## Description ##

OpenApiLINQPadDriver is LINQPad 7 dynamic data context driver for creating C# clients based on [Open API](https://www.openapis.org)/[Swagger](https://swagger.io/specification/) specifications

* Specification is read using [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) and clients are generated using [NSwag](https://github.com/RicoSuter/NSwag)

## Websites ##

* [This project](https://github.com/romfir/OpenApiLINQPadDriver)
* [Original project for LINQPad 5](https://github.com/seba76/SwaggerContextDriver)
* UI is heavily inspired by [CsvLINQPadDriver](https://github.com/i2van/CsvLINQPadDriver)

## Downloads ##
generation of lpx6 files is on the roadmap, for now we only support instalation via nuget

## Prerequisites ##

* [LINQPad 7](https://www.linqpad.net/LINQPad7.aspx): [.NET 7](https://dotnet.microsoft.com/download/dotnet/7.0)/[.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)

## Installation ##

### LINQPad 7 ###

#### NuGet ####

[![NuGet](https://img.shields.io/nuget/v/OpenApiLINQPadDriver)](https://www.nuget.org/packages/OpenApiLINQPadDriver)

* Open LINQPad 7.
* Click `Add connection` link.
* Click button `View more drivers...`
* Click radio button `Show all drivers` and type `OpenApiLINQPadDriver` (for now it is also required to check `Include Prerelease` checkbox)
* Install.
* In case of working in environments without internet access it is possible to manually download nuget package and configure `Package Source` to point to a folder where it is located

## Usage ##

Open API Connection can be added the same way as any other connection.

* Click `Add Connection`
* Select `OpenApi Driver`
* Enter `Open Api Json Uri` or click `Get from disk` and pick it from file
* Manually enter `API Uri` or click `Get from swagger.json`, if servers are found in the specification uri of the first one will be picked
* Set settings
* Click `OK`
* Client should start generation, you can use it by right clicking on it and choosing `Use in Current Query` or by picking it from `Connection` select
* It is possible to drag method name from the tree view on the left to the query
* Example code using [PetStore API](https://petstore.swagger.io/v2/swagger.json)
```csharp
async Task Main() 
{
  var newPetId = System.Random.Shared.NextInt64();
  await PetClient.AddPetAsync(new Pet() 
  {
    Id = newPetId,
    Name = "Dino",
    Category = new Category 
    {
        Id = 123,
        Name = "Dog"
    }
  });

  await PetClient.GetPetByIdAsync(newPetId).Dump();
}
```
### Refreshing client ###
* Right click on the connection and click `Refresh`
* Or use shortcut `Shift+Alt+D`

## Configuration Options ##

### Client Generation ###

* Endpoint grouping - how methods will be grouped in generated client
  * `Multiple clients from first tag and operationName` - usually first tag corresponds to ASP.NET controller, so this will group methods by controller
  * `Single client from OperationId and OperationName` - this will put all endpoints in one class
* Json library - library used in generated client for serialization/deserialization, for specification reading [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) uses `Newstonsoft.Json`
    * `System.Text.Json`
    * `Newstonsoft.Json`
* Class style
    * `POCOs (Plain Old C# Objects)`
    * `Classes implementing the INotifyPropertyChanged interface`
    * `Classes implementing the Prism base class` - WIP, the library that is required is not added to the compilation
    * `Records - read only POCOs (Plain Old C# Objects)`
* Generate sync methods - by default sync methods will not be generated

### Misc ###

* Debug info: show additional driver debug info, e.g. generated data context sources and add `Execution Times` explorer item with exectuion times of parts of the generation pipeline
* Remember this connection: connection will be available on next run.
* Contains production data: files contain production data.

### PrepareRequestFunction ###
* Each generated client has `PrepareRequestFunction` Func, for  `Multiple clients from first tag and operationName` mode, helper set only Func is also generted to set them all at once
* This Func will be run on each method exectuion before making a http request, it is run in `PrepareRequest` partial methods generated by [NSwag](https://github.com/RicoSuter/NSwag)
* It can be used to set additional headers or other http client settings
* Example usage
```csharp
async Task Main() 
{
  PrepareRequestFunction = (httpClient, requestMessage, url) => 
  {
    requestMessage.Headers.Add("UserId", "9");
    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<token>");
  };
}
```
```csharp
async Task Main() 
{
  PrepareRequestFunction = PrepareRequest;
}

private void PrepareRequest(HttpClient httpClient, HttpRequestMessage requestMessage, string url) 
{
  requestMessage.Headers.Add("UserId", "9");
  requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<token>");
}
```
### Known Issues ###
* Code does not compile when there are parameters with the same name, but with case differences in the single endpoint and they come from different locations eg `[FromQuery] int test, [FromHeader] int Test` [related issue](https://github.com/RicoSuter/NSwag/issues/2560)

### Development ###
* [OpenApiLINQPadDriver.csproj](https://github.com/romfir/OpenApiLINQPadDriver/blob/master/OpenApiLINQPadDriver/OpenApiLINQPadDriver.csproj) contains special `Debug_Publish_To_LINQPad_Folder` debug build configuration, if it is chosen, code will be build only targeting `net7.0-windows` with additional properties:
https://github.com/romfir/OpenApiLINQPadDriver/blob/0ce72bb692b39088360613123b08cdf8a51ec506/OpenApiLINQPadDriver/OpenApiLINQPadDriver.csproj#L50-L56
  * LINQPad can pick drivers from `\LINQPad\Drivers\DataContext\NetCore` folder
  * Additionaly when exceptions will be thrown it will be possible to attach a debugger:
https://github.com/romfir/OpenApiLINQPadDriver/blob/0ce72bb692b39088360613123b08cdf8a51ec506/OpenApiLINQPadDriver/OpenApiContextDriver.cs#L11-L20

### Roadmap ###
* Allow injection of own httpClient
* Unit tests
* `PrepareRequest` with string builder overload
* `ProcessResponse`
* `PrepareRequest` and `ProcessResponse` async overload that could be set via a setting
* Methods parameters and responses in tree view
* Auto dump response
* Auth helper methods eg. `SetBearerToken`
* Check if it is possible to add summary to generated methods
* Add `Prism.Mvvm` to compilation when prism class style is picked
* When multiple servers are found allow selection
* LINQPad 5 support
* Examples (include in the nuget)
