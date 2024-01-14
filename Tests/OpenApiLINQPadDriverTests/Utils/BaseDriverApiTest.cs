using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace OpenApiLINQPadDriverTests.Utils;

public abstract class BaseDriverApiTest : IAsyncLifetime
{
    private const string DriverName = nameof(OpenApiLINQPadDriver);
    private WebApplication? _webApplication;

    private string? _apiUri;
    private string ApiUrl => _apiUri ?? throw new ArgumentNullException(nameof(_apiUri));
    private static readonly string BaseDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath)!;

    private readonly List<Action<WebApplication>> _endpoints = [];
    private readonly QueryExecutor? _queryExecutor = new();

    protected void StartApi(bool removeServerFromApiDefinition = false)
    {
        LinqPadHelper.ThrowIfDriverExists(DriverName);

        InstallDriver();

        var builder = WebApplication.CreateBuilder();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            if (!removeServerFromApiDefinition)
            {
                options.AddServer(new OpenApiServer
                {
                    Url = ApiUrl
                });
            }
        });

        //by using :0 port an empty one will be assigned
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        var app = builder.Build();

        app.Lifetime.ApplicationStarted.Register(() => _apiUri = app.Urls.Single());

        app.UseSwagger();

        foreach (var endpoint in _endpoints)
        {
            endpoint.Invoke(app);
        }

        app.RunAsync();

        _webApplication = app;
    }

    private static void InstallDriver()
    {
        var files = new List<string> { DriverName + ".dll", GetDepsJsonRelativePath(DriverName + ".dll", "Tests") };

        var driverOutputPath = Path.Combine("Drivers", "DataContext", "NetCore", DriverName);

        Directory.CreateDirectory(driverOutputPath);

        foreach (var file in files)
        {
            CopyFile(file);
        }

        return;

        void ExecIfFileIsNewer(string file, Action<string, string> action)
        {
            var srcFile = Path.GetFullPath(file);
            var dstFile = Path.Combine(driverOutputPath, Path.GetFileName(file));

            var srcFileInfo = new FileInfo(srcFile);
            var dstFileInfo = new FileInfo(dstFile);

            if (!dstFileInfo.Exists || dstFileInfo.LastWriteTime < srcFileInfo.LastWriteTime)
            {
                action(srcFile, dstFile);
            }
        }

        void CopyFile(string file) =>
            ExecIfFileIsNewer(file, (srcFile, dstFile) => File.Copy(srcFile, dstFile, true));
    }

    protected Task<string> ExecuteScriptAsync(string script, EndpointGrouping endpointGrouping, JsonLibrary jsonLibrary, ClassStyle classStyle)
        => _queryExecutor!.RunAsync(ApiUrl, script, endpointGrouping, jsonLibrary, classStyle, OpenApiFormat.Json);

    protected void MapGet(string controllerName, string pattern, string name, Delegate handler)
        => _endpoints.Add(app =>
           app.MapGet(pattern, handler)
               .WithName(name)
               .WithOpenApi(openApiConfig =>
                {
                    openApiConfig.Tags.Clear();
                    openApiConfig.Tags.Add(new OpenApiTag
                    {
                        Name = controllerName
                    });
                    return openApiConfig;
                }));

    private static string GetDepsJsonRelativePath(string driverFileName, string testsFolderPath)
        => GetDepsJsonRelativePath(driverFileName, baseDir => baseDir.Replace(testsFolderPath, string.Empty, StringComparison.OrdinalIgnoreCase));

    private static string GetDepsJsonRelativePath(string driverFileName, Func<string, string> getDepsJsonFileFullPath)
        => Path.Combine(Path.GetRelativePath(BaseDir, Path.Combine(getDepsJsonFileFullPath(BaseDir), Path.ChangeExtension(driverFileName, ".deps.json"))));

    Task IAsyncLifetime.InitializeAsync()
        => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_webApplication != null)
            await _webApplication.DisposeAsync();

        _queryExecutor?.Dispose();
    }
}
