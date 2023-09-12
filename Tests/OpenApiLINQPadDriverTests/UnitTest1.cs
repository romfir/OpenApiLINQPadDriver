using FluentAssertions;
using LINQPad.Extensibility.DataContext;
using LPRun;
using Moq;
using OpenApiLINQPadDriver;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenApiServer = Microsoft.OpenApi.Models.OpenApiServer;

namespace OpenApiLINQPadDriverTests;

public class UnitTest1 : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _apiFixture;
    public UnitTest1(ApiFixture apiFixture) => _apiFixture = apiFixture;

    [Fact]
    public async Task Test1()
    {
        var connectionInfoMock = new Mock<IConnectionInfo>();

        connectionInfoMock.Setup(s => s.DriverData).Returns(XDocument.Parse(@$"    <DriverData>
    <OpenApiDocumentUri>{_apiFixture.ApiUrl}swagger/v1/swagger.json</OpenApiDocumentUri>
    <ApiUri>{_apiFixture.ApiUrl}</ApiUri>
</DriverData>").Root!);

        var driverProperties = new OpenApiContextDriverProperties(connectionInfoMock.Object);
        var linqScriptName = "Test";


        var queryConfig = GetQueryHeaders().Aggregate(new StringBuilder(), static (stringBuilder, header) =>
        {
            if (ShouldRender(header))
            {
                stringBuilder.AppendLine(header);
                stringBuilder.AppendLine();
            }

            return stringBuilder;
        }).ToString();

        var linqScript = LinqScript.Create(
            $"{linqScriptName}.linq",
            queryConfig,
            linqScriptName);


        // Act: Execute test LNQPad script.
        var (output, error, exitCode) =
             Runner.Execute(linqScript);

        // Assert.
        error.Should().BeNullOrWhiteSpace();
        exitCode.Should().Be(0);

        IEnumerable<string> GetQueryHeaders()
        {
            // Connection header.
            var nameSpace = nameof(OpenApiLINQPadDriver);
            var driverTypeName = nameof(OpenApiContextDriver);
            yield return ConnectionHeader.Get(
                nameSpace,
                $"{nameSpace}.{driverTypeName}",
                driverProperties,
                "System.Runtime.CompilerServices");

            // FluentAssertions helper.
            yield return
                @"string Reason([CallerLineNumber] int sourceLineNumber = 0) =>" +
                @" $""something went wrong at line #{sourceLineNumber}"";";

            // Test context.
            //if (!string.IsNullOrWhiteSpace(context))
            //{
            //    yield return $"var context = {context};";
            //}
        }
    }
    static bool ShouldRender(string? str) =>
        !string.IsNullOrWhiteSpace(str);

}

public class ApiFixture : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public readonly string ApiUrl = "https://localhost:5003/";
    public ApiFixture()
    {
        try
        {
            const string driverName = "OpenApiLINQPadDriver";
            var linqPadFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LINQPad");
            var nugetDriverPath = Path.Combine(linqPadFolderPath, "NuGet.Drivers", driverName);
            var folderDriverPath = Path.Combine(linqPadFolderPath, "Drivers", "DataContext", driverName);
            if (Directory.Exists(nugetDriverPath))
                throw new InvalidOperationException($"Driver already exists inside \"{nugetDriverPath}\", please remove it via LINQPad");
            if (Directory.Exists(folderDriverPath))
                throw new InvalidOperationException($"Driver already exists inside \"{folderDriverPath}\", please remove it via LINQPad");

            // Copy driver to LPRun drivers folder.
            Driver.InstallWithDepsJson(
                driverName,
                driverName + ".dll",
                "Tests"
            );


            var builder = WebApplication.CreateBuilder();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddServer(new OpenApiServer
                {
                    Url = ApiUrl
                });
            });
            builder.Services.AddMvc();

            builder.WebHost
                .UseUrls(ApiUrl);

            var app = builder.Build();


            app.UseSwagger(c =>
            {
            });
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.MapGet("/weatherforecast", (int numberOf, string summary) =>
                {
                    var forecast = Enumerable.Range(1, numberOf).Select(index =>
                            new WeatherForecast
                            (
                                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                numberOf,
                                summary
                            ))
                        .ToArray();
                    return forecast;
                })
                .WithName("GetWeatherForecast")
                .WithOpenApi(config =>
                {
                    config.Tags.Clear();
                    config.Tags.Add(new OpenApiTag
                    {
                        Name = "Weather"
                    });
                    return config;
                });


            app.RunAsync(_cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }
}