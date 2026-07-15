using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
#if DEBUG_PUBLISH_TO_LINQPAD_FOLDER
using System;
using System.Diagnostics;
#endif

namespace OpenApiLINQPadDriver;
// ReSharper disable once UnusedType.Global
public class OpenApiContextDriver : DynamicDataContextDriver
{
#if DEBUG_PUBLISH_TO_LINQPAD_FOLDER
    private const string DriverName = "OpenApi Driver from folder";
    public OpenApiContextDriver()
    {
        AppDomain.CurrentDomain.FirstChanceException += (_, args) =>
        {
            if (args.Exception.StackTrace?.Contains(nameof(OpenApiLINQPadDriver)) == true)
                Debugger.Launch();
        };
    }
#else
    private const string DriverName = "OpenApi Driver";
#endif

    public override string GetConnectionDescription(IConnectionInfo cxInfo)
        => "OpenApi - " + new OpenApiContextDriverProperties(cxInfo).ApiUri;

    public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions)
    {
        var dialogProperties = new OpenApiContextDriverProperties(cxInfo);

        if (new ConnectionDialog(dialogProperties).ShowDialog() != true)
            return false;

        cxInfo.Persist = dialogProperties.Persist;
        cxInfo.IsProduction = dialogProperties.IsProduction;
        cxInfo.DisplayName = GetConnectionDescription(cxInfo);

        return true;
    }

    public override string Name => DriverName;

    public override string Author =>
#pragma warning disable CS8603 // Possible null reference return.
        typeof(OpenApiContextDriver).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == "Authors").Value;
#pragma warning restore CS8603 // Possible null reference return.

    public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        => SchemaBuilder.GetSchemaAndBuildAssembly(new OpenApiContextDriverProperties(cxInfo), assemblyToBuild, ref nameSpace, ref typeName);

    public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        =>
        [
            ParameterDescriptors.HttpClient
        ];

    public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        => [new OpenApiHttpClient()];

    public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
    {
    }

    public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        => ((HttpClient)constructorArguments[0]).Dispose();

    public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
       => OpenApiContextDriverPropertiesEqualityComparer.Default.Equals(new OpenApiContextDriverProperties(c1), new OpenApiContextDriverProperties(c1));
}
