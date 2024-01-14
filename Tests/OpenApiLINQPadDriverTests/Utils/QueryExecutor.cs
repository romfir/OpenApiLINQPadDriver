using LINQPad.Extensibility.DataContext;
using Moq;
using OpenApiLINQPadDriver;
using System.Text;
using System.Xml.Linq;
using LINQPad;
using System.Globalization;

namespace OpenApiLINQPadDriverTests.Utils;
internal sealed class QueryExecutor : IDisposable
{
    private const string DriverNameSpace = nameof(OpenApiLINQPadDriver);
    private const string DriverTypeName = nameof(OpenApiContextDriver);
    private const string TempScriptDirectory = "TempScripts";

    private readonly string _uniqueFileName = Guid.NewGuid().ToString();
    private bool _wasRun;

    public async Task<string> RunAsync(string apiUri, string script, EndpointGrouping endpointGrouping, JsonLibrary jsonLibrary, ClassStyle classStyle, OpenApiFormat openApiFormat)
    {
        //otherwise LINQPad errors are localized
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        if (_wasRun)
            throw new InvalidOperationException("Multiple script execution per test is not supported (to support it create list of unique file names and then dispose of them");
        _wasRun = true;

        var queryConfig = GetQueryHeaders(apiUri, script, endpointGrouping, jsonLibrary, classStyle, openApiFormat)
            .Aggregate(new StringBuilder(), static (stringBuilder, header) =>
        {
            stringBuilder.AppendLine(header);
            stringBuilder.AppendLine();

            return stringBuilder;
        }).ToString();

        Directory.CreateDirectory(TempScriptDirectory);
        var filePath = GetFilePath();
        await File.WriteAllTextAsync(filePath, queryConfig);

        try
        {
          // await Task.Delay(TimeSpan.FromMinutes(20));

            using var compilation = await Util.CompileAsync(filePath); //, forceRebuild: true, cxName: "OpenApiContextDriver"
            var queryExecuter =  compilation.Run(QueryResultFormat.Html);
            //var exception = await queryExecuter.ExceptionAsync;
            var returnValue = await queryExecuter.ReturnValueAsync;
            
            //exception.Should().BeNull();
            return "";

        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }

    private static IEnumerable<string> GetQueryHeaders(string apiUri, string script, EndpointGrouping endpointGrouping, JsonLibrary jsonLibrary, ClassStyle classStyle, OpenApiFormat openApiFormat)
    {
        var connectionInfoMock = new Mock<IConnectionInfo>();

        var swaggerPath = Path.Join(apiUri, "/swagger/v1/swagger.json");

        connectionInfoMock.Setup(s => s.DriverData).Returns(XDocument.Parse(
            $"""
             <DriverData>
                <{nameof(OpenApiContextDriverProperties.OpenApiFormat)}>{openApiFormat}</{nameof(OpenApiContextDriverProperties.OpenApiFormat)}>
                <{nameof(OpenApiContextDriverProperties.OpenApiDocumentUri)}>{swaggerPath}</{nameof(OpenApiContextDriverProperties.OpenApiDocumentUri)}>
                <{nameof(OpenApiContextDriverProperties.ApiUri)}>{apiUri}</{nameof(OpenApiContextDriverProperties.ApiUri)}>
                <{nameof(OpenApiContextDriverProperties.EndpointGrouping)}>{endpointGrouping}</{nameof(OpenApiContextDriverProperties.EndpointGrouping)}>
                <{nameof(OpenApiContextDriverProperties.DebugInfo)}>true</{nameof(OpenApiContextDriverProperties.DebugInfo)}>
                <{nameof(OpenApiContextDriverProperties.JsonLibrary)}>{jsonLibrary}</{nameof(OpenApiContextDriverProperties.JsonLibrary)}>
                <{nameof(OpenApiContextDriverProperties.ClassStyle)}>{classStyle}</{nameof(OpenApiContextDriverProperties.ClassStyle)}>
            </DriverData>
            """).Root!);

        var driverProperties = new OpenApiContextDriverProperties(connectionInfoMock.Object);

        yield return ConnectionHeader.Get(
            DriverNameSpace,
            $"{DriverNameSpace}.{DriverTypeName}",
            driverProperties,
            "System.Runtime.CompilerServices", "FluentAssertions.Specialized", "System.Threading.Tasks");

        //todo add some ifs Task/Task<object>
        yield return "async Task<object> Main (object[] args)";
        yield return "{";
        yield return script;
        yield return "return null;";
        yield return "}";
        yield return
            "string Reason([CallerLineNumber] int sourceLineNumber = 0) =>" +
            """ $"something went wrong at line #{sourceLineNumber}";""";
    }

    private string GetFilePath() => Path.Join(TempScriptDirectory, _uniqueFileName + ".linq");

    public void Dispose()
    {
        //#if DEBUG // on CI Github runners are purged after each run so we don't care about leaving these files
        //File.Delete(GetFilePath());
        //#endif
    }
}
