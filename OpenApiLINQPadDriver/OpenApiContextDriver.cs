﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using LINQPad.Extensibility.DataContext;

namespace OpenApiLINQPadDriver;
// ReSharper disable once UnusedMember.Global
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
        {
            return false;
        }

        cxInfo.Persist = dialogProperties.Persist;
        cxInfo.IsProduction = dialogProperties.IsProduction;
        cxInfo.DisplayName = GetConnectionDescription(cxInfo);

        return true;
    }

    public override string Name => DriverName;

    // ReSharper disable StringLiteralTypo
    public override string Author => "Damian Romanowski (romfir22@gmail.com)";
    // ReSharper restore StringLiteralTypo

    public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        => SchemaBuilder.GetSchemaAndBuildAssembly(new OpenApiContextDriverProperties(cxInfo), assemblyToBuild, ref nameSpace, ref typeName);

    public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        =>
        [
            ParameterDescriptors.HttpClient
        ];

    public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        => [new HttpClient()];

    public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
    {
    }

    public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
    {
        ((HttpClient)constructorArguments[0]).Dispose();
    }

    public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
       => OpenApiContextDriverPropertiesEqualityComparer.Default.Equals(new OpenApiContextDriverProperties(c1), new OpenApiContextDriverProperties(c1));
}
