﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using OpenApiLINQPadDriver.Compilation;
using OpenApiLINQPadDriver.Enums;
using OpenApiLINQPadDriver.Extensions;

namespace OpenApiLINQPadDriver;
internal static class SchemaBuilder
{
    private const string ClientPostFix = "Client";
    private const string TypedDataContextName = "ApiClient";

    internal static List<ExplorerItem> GetSchemaAndBuildAssembly(OpenApiContextDriverProperties driverProperties, AssemblyName assemblyToBuild, ref string nameSpace,
        ref string typeName)
    {
        typeName = TypedDataContextName;
        var mainContextType = new TypeDescriptor(typeName, nameSpace);
        var timeExplorerItem = ExplorerItemHelper.CreateForTimeMeasurement();

#if NET7_0_OR_GREATER
        var initialTimeStamp = Stopwatch.GetTimestamp();
#else
        var stopWatch = Stopwatch.StartNew();
#endif
        var document = OpenApiDocumentHelper.GetFromUri(new Uri(driverProperties.OpenApiDocumentUri!), driverProperties.OpenApiFormat);

        MeasureTimeAndAddTimeExecutionExplorerItem("Downloading document");

        document.SetServer(driverProperties.ApiUri!);

        var endpointGrouping = driverProperties.EndpointGrouping;
        var classStyle = driverProperties.ClassStyle;
        var settings = CreateCsharpClientGeneratorSettings(endpointGrouping, driverProperties.JsonLibrary, classStyle, driverProperties.GenerateSyncMethods, mainContextType);

        var generator = new CSharpClientGenerator(document, settings);

        var codeGeneratedByNSwag = generator.GenerateFile();

        MeasureTimeAndAddTimeExecutionExplorerItem("Generating NSwag classes");

        var clientSourceCode = endpointGrouping switch
        {
            EndpointGrouping.SingleClientFromOperationIdOperationName => ClientGenerator.SingleClientFromOperationIdOperationNameGenerator(mainContextType),
            EndpointGrouping.MultipleClientsFromFirstTagAndOperationName => ClientGenerator.MultipleClientsFromOperationIdOperationNameGenerator(GetClientNames(), mainContextType),
            _ => throw new InvalidOperationException()
        };

        MeasureTimeAndAddTimeExecutionExplorerItem("Generating clients partials");

        var references = driverProperties.GetCoreFxReferenceAssemblies()
            .Append(typeof(JsonConvert).Assembly.Location) //required for code generation, otherwise NSwag will use the lowest possible version 10.0.1
            .AppendIf(classStyle == ClassStyle.Prism, typeof(Prism.IActiveAware).Assembly.Location)
            .ToArray();

#pragma warning disable SYSLIB0044 //this is the only way to read this assembly, LINQPad does not give any other reference to it
        var assemblyPath = assemblyToBuild.CodeBase!;
#pragma warning restore SYSLIB0044
        var compileResult = RoslynHelper.CompileSource(new CompilationInput
        {
            FilePathsToReference = references,
            OutputPath = assemblyPath,
            SourceCode = [codeGeneratedByNSwag, clientSourceCode]
        }, driverProperties.BuildInRelease);

        MeasureTimeAndAddTimeExecutionExplorerItem("Compiling code");

        var explorerItems = new List<ExplorerItem>();

        if (driverProperties.DebugInfo)
        {
            explorerItems.Add(timeExplorerItem);

            if (compileResult.Warnings.Length > 0)
            {
                explorerItems.Add(ExplorerItemHelper.CreateForCompilationWarnings(compileResult.Warnings));
            }
        }

        if (compileResult.Errors.Length > 0 || driverProperties.DebugInfo)
            explorerItems.AddRange(ExplorerItemHelper.CreateForGeneratedCode(codeGeneratedByNSwag, clientSourceCode));

        if (compileResult.Errors.Length > 0)
        {
            explorerItems.Add(ExplorerItemHelper.CreateForCompilationErrors(compileResult.Errors));
            return explorerItems;
        }

        var assemblyWithGeneratedCode = DataContextDriver.LoadAssemblySafely(assemblyPath);

        MeasureTimeAndAddTimeExecutionExplorerItem("Loading assembly from file");

        var contextType = assemblyWithGeneratedCode.GetType(mainContextType);

        explorerItems.AddRange(ReflectionSchemaBuilder.GenerateExplorerItems(contextType, endpointGrouping));

        MeasureTimeAndAddTimeExecutionExplorerItem("Reading assembly using reflection and generating schema");

        return explorerItems;

        List<string> GetClientNames()
        {
            var firstOperationTags = new HashSet<string>();
            foreach (var operationDescription in document.Operations)
            {
                var firstOperationTagOrNull = operationDescription.Operation.Tags.FirstOrDefault();

                firstOperationTags.Add(firstOperationTagOrNull ?? ClientPostFix);
            }

            return firstOperationTags.Select(tag => tag == ClientPostFix ? tag : settings.GenerateControllerName(tag))
                .ToList();
        }

        void MeasureTimeAndAddTimeExecutionExplorerItem(string name)
        {
#if NET7_0_OR_GREATER
            var temp = initialTimeStamp;
            initialTimeStamp = Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(temp, initialTimeStamp);
#else
            var elapsed = stopWatch.Elapsed;
            stopWatch.Restart();
#endif
            timeExplorerItem.Children.Add(ExplorerItemHelper.CreateForElapsedTime(name, elapsed));
        }
    }

    private static CSharpClientGeneratorSettings CreateCsharpClientGeneratorSettings(EndpointGrouping endpointGrouping, JsonLibrary jsonLibrary, ClassStyle classStyle, bool generateSyncMethods,
        TypeDescriptor type)
    {
        var (operationNameGenerator, className) = endpointGrouping switch
        {
            EndpointGrouping.MultipleClientsFromFirstTagAndOperationName => ((IOperationNameGenerator)new MultipleClientsFromFirstTagAndOperationNameGenerator(), "{controller}" + ClientPostFix),
            EndpointGrouping.SingleClientFromOperationIdOperationName => (new SingleClientFromOperationIdOperationNameGenerator(), type.Name),
            _ => throw new InvalidOperationException()
        };

        var settings = new CSharpClientGeneratorSettings
        {
            GenerateClientClasses = true,
            GenerateOptionalParameters = true,
            ClassName = className,
            OperationNameGenerator = operationNameGenerator,
            CSharpGeneratorSettings =
            {
                Namespace = type.NameSpace,
                JsonLibrary = (CSharpJsonLibrary)jsonLibrary,
                ClassStyle = (CSharpClassStyle)classStyle,
            },
            UseHttpClientCreationMethod = false,
            DisposeHttpClient = true,
            GenerateSyncMethods = generateSyncMethods,
            GeneratePrepareRequestAndProcessResponseAsAsyncMethods = false,
        };
        return settings;
    }
}
