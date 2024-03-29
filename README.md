# OpenApiLINQPadDriver for LINQPad 7/8
[![Latest build](https://github.com/romfir/OpenApiLINQPadDriver/workflows/Build/badge.svg)](https://github.com/romfir/OpenApiLINQPadDriver/actions)
[![NuGet](https://img.shields.io/nuget/v/OpenApiLINQPadDriver)](https://www.nuget.org/packages/OpenApiLINQPadDriver)
[![Downloads](https://img.shields.io/nuget/dt/OpenApiLINQPadDriver)](https://www.nuget.org/packages/OpenApiLINQPadDriver)
[![License](https://img.shields.io/badge/license-MIT-yellow)](https://opensource.org/licenses/MIT)

## Description ##

OpenApiLINQPadDriver is LINQPad 7/8 dynamic data context driver for creating C# clients based on [Open API](https://www.openapis.org)/[Swagger](https://swagger.io/specification/) specifications

* Specification is read using [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) and clients are generated using [NSwag](https://github.com/RicoSuter/NSwag)

## Websites ##

* [This project](https://github.com/romfir/OpenApiLINQPadDriver)
* [Original project for LINQPad 5](https://github.com/seba76/SwaggerContextDriver)
* UI is heavily inspired by [CsvLINQPadDriver](https://github.com/i2van/CsvLINQPadDriver)

## Downloads ##
generation of lpx6 files is on the roadmap, for now we only support instalation via nuget

## Prerequisites ##

* [LINQPad 8](https://www.linqpad.net/LINQPad7.aspx): [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
* [LINQPad 7](https://www.linqpad.net/LINQPad7.aspx): [.NET 7](https://dotnet.microsoft.com/download/dotnet/7.0)/[.NET 6](https://dotnet.microsoft.com/download/dotnet/6.0)

## Installation ##

### LINQPad 7/8 ###

#### NuGet ####

[![NuGet](https://img.shields.io/nuget/v/OpenApiLINQPadDriver)](https://www.nuget.org/packages/OpenApiLINQPadDriver)

* Open LINQPad 7/8.
* Click `Add connection` link.
* Click button `View more drivers...`
* Click radio button `Show all drivers` and type `OpenApiLINQPadDriver` (for now it is also required to check `Include Prerelease` checkbox)
* Install.
* In case of working in environments without internet access it is possible to manually download nuget package and configure `Package Source` to point to a folder where it is located

## Usage ##

Open API Connection can be added the same way as any other connection.

* Click `Add Connection`
* Select `OpenApi Driver`
* Enter `Open Api/Swagger Uri` or click `Get from disk` and pick it from file
* Manually enter `API Uri` or click `Get from Open Api document`, if servers are found in the specification then uri of the first one will be picked
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
    * `Classes implementing the Prism base class`
    * `Records - read only POCOs (Plain Old C# Objects)`
* Generate sync methods - by default sync methods will not be generated
* Build in Release - Build generated code in Release, default: `false`

### Misc ###

* Debug info: show additional driver debug info, e.g. generated data context sources, add `Execution Times` explorer item with execution times of parts of the generation pipeline and will add warnings from the compilation if any were present
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
## Credits ##

### Tools ###

* [LINQPad 7](https://www.linqpad.net/LINQPad7.aspx)
* [LINQPad 8](https://www.linqpad.net/LINQPad8.aspx)
* [LINQPad Command-Line and Scripting (LPRun)](https://www.linqpad.net/lprun.aspx)

### Libraries ###

* [NJsonSchema](https://github.com/RicoSuter/NJsonSchema)
* [NSwag](https://github.com/RicoSuter/NSwag)
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - required by NSwag, included to bump version

### Development ###
* [OpenApiLINQPadDriver.csproj](https://github.com/romfir/OpenApiLINQPadDriver/blob/master/OpenApiLINQPadDriver/OpenApiLINQPadDriver.csproj) contains special `Debug_Publish_To_LINQPad_Folder` debug build configuration, if it is chosen, code will be build only targeting `net8.0-windows` with [additional properties](https://github.com/romfir/OpenApiLINQPadDriver/blob/master/OpenApiLINQPadDriver/OpenApiLINQPadDriver.csproj?plain=1#L52-L57)
* LINQPad can pick drivers from `\LINQPad\Drivers\DataContext\NetCore` folder
* [Additionaly when exceptions will be thrown it will be possible to attach a debugger](https://github.com/romfir/OpenApiLINQPadDriver/blob/master/OpenApiLINQPadDriver/OpenApiContextDriver.cs?plain=1#L12-L21)

### Roadmap ###
* Allow injection of own httpClient
* Unit tests
* `PrepareRequest` with string builder overload
* `ProcessResponse`
* `PrepareRequest` and `ProcessResponse` async overload that could be set via a setting
* Methods parameters and responses in tree view
* Auto dump response
* Auth helper methods eg. `SetBearerToken`
* When multiple servers are found allow selection
* LINQPad 5 support
* Examples (include in the nuget) - possibly the same ones could be used in testing
* Expose JsonSerializerSettings setter on multi client setup
* Expose ReadResponseAsString on multi client setup
* Treat warnings as errors in generated code (`generalDiagnosticOption: ReportDiagnostic.Error`)
