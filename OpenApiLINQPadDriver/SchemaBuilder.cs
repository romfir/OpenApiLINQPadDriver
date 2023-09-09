using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
using Newtonsoft.Json;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using OpenApiLINQPadDriver.Enums;
using OpenApiLINQPadDriver.Extensions;

namespace OpenApiLINQPadDriver;
internal static class SchemaBuilder
{
    private const string ClientPostFix = "Client";
    private const string TypedDataContextName = "ApiClient";

    internal static List<ExplorerItem> GetSchemaAndBuildAssembly(OpenApiContextDriverProperties openApiContextDriverProperties, AssemblyName assemblyToBuild, ref string nameSpace,
        ref string typeName)
    {
        typeName = TypedDataContextName;

        var timeExplorerItem = CreateExplorerItemForTimeMeasurement();
        var stopWatch = Stopwatch.StartNew();

        var document = AsyncHelper.RunSync(() => OpenApiDocumentHelper.GetFromUriAsync(new Uri(openApiContextDriverProperties.OpenApiDocumentUri!)));

        MeasureTimeAndRestartStopWatch("Downloading document");

        document.SetServer(openApiContextDriverProperties.ApiUri!);

        var endpointGrouping = openApiContextDriverProperties.EndpointGrouping;
        var settings = CreateCsharpClientGeneratorSettings(endpointGrouping, openApiContextDriverProperties.JsonLibrary, openApiContextDriverProperties.ClassStyle,
            openApiContextDriverProperties.GenerateSyncMethods, nameSpace, typeName);

        var generator = new CSharpClientGenerator(document, settings);

        var codeGeneratedByNSwag = generator.GenerateFile();

        MeasureTimeAndRestartStopWatch("Generating NSwag classes");

        var clientSourceCode = endpointGrouping switch
        {
            EndpointGrouping.SingleClientFromOperationIdOperationName => ClientGenerator.SingleClientFromOperationIdOperationNameGenerator(nameSpace, typeName),
            EndpointGrouping.MultipleClientsFromFirstTagAndOperationName => ClientGenerator.MultipleClientsFromOperationIdOperationNameGenerator(GetClientNames(), nameSpace, typeName),
            _ => throw new InvalidOperationException()
        };

        MeasureTimeAndRestartStopWatch("Generating clients partials");

        var compileResult = DataContextDriver.CompileSource(new CompilationInput
        {
            FilePathsToReference = openApiContextDriverProperties.GetCoreFxReferenceAssemblies()
                .Append(typeof(JsonConvert).Assembly.Location) //required for code generation, otherwise NSwag will use lowest possible version 10.0.1
                .ToArray(),
#pragma warning disable SYSLIB0044 //this is the only way to read this assembly, LINQPad does not give any other reference to it
            OutputPath = assemblyToBuild.CodeBase,
            SourceCode = new[] { codeGeneratedByNSwag, clientSourceCode }
        });

        MeasureTimeAndRestartStopWatch("Compiling code");

        var explorerItems = new List<ExplorerItem>();

        if (openApiContextDriverProperties.DebugInfo)
            explorerItems.Add(timeExplorerItem);

        if (compileResult.Errors.Any() || openApiContextDriverProperties.DebugInfo)
        {
            explorerItems.AddRange(CreateExplorerItemsWithGeneratedCode(codeGeneratedByNSwag, clientSourceCode));
        }

        if (compileResult.Errors.Any())
        {
            explorerItems.Add(GenerateErrorExplorerItem(compileResult.Errors));
            return explorerItems;
        }

        var assemblyWithGeneratedCode = Assembly.LoadFile(assemblyToBuild.CodeBase!);
#pragma warning restore SYSLIB0044

        MeasureTimeAndRestartStopWatch("Loading assembly from file");

        var contextType = assemblyWithGeneratedCode.GetType(nameSpace, TypedDataContextName);

        explorerItems.AddRange(ReflectionSchemaBuilder.GenerateExplorerItems(contextType, endpointGrouping));

        MeasureTimeAndRestartStopWatch("Reading assembly using reflection and generating schema");

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

        static ExplorerItem GenerateErrorExplorerItem(IReadOnlyCollection<string> errors)
        {
            var errorExplorerItem = new ExplorerItem("Compile errors", ExplorerItemKind.Schema, ExplorerIcon.LinkedDatabase)
            {
                Children = new List<ExplorerItem>(errors.Count)
            };

            foreach (var error in errors)
            {
                errorExplorerItem.Children.Add(new ExplorerItem(error, ExplorerItemKind.Schema, ExplorerIcon.Inherited)
                {
                    DragText = error,
                    ToolTipText = error
                });
            }

            return errorExplorerItem;
        }

        ExplorerItem CreateExplorerItemForTimeMeasurement()
            => new("Execution Times", ExplorerItemKind.Schema, ExplorerIcon.Box)
            {
                Children = new List<ExplorerItem>()
            };

        void MeasureTimeAndRestartStopWatch(string name)
        {
            timeExplorerItem.Children.Add(new ExplorerItem(name + " " + stopWatch.Elapsed, ExplorerItemKind.Property, ExplorerIcon.Blank));
            stopWatch.Restart();
        }

        static List<ExplorerItem> CreateExplorerItemsWithGeneratedCode(string codeGeneratedByNSwag, string clientSourceCode)
            => new()
            {
                new("NSwag generated source code", ExplorerItemKind.Schema, ExplorerIcon.Schema)
                {
                    ToolTipText = "Drag and drop context generated source code to text window",
                    DragText = codeGeneratedByNSwag
                },
                new("Client source code", ExplorerItemKind.Schema, ExplorerIcon.Schema)
                {
                    ToolTipText = "Drag and drop context source code to text window client",
                    DragText = clientSourceCode
                }
            };
    }

    private static CSharpClientGeneratorSettings CreateCsharpClientGeneratorSettings(EndpointGrouping endpointGrouping, JsonLibrary jsonLibrary, ClassStyle classStyle, bool generateSyncMethods,
        string nameSpace, string linqPadTypeName)
    {
        var (operationNameGenerator, className) = endpointGrouping switch
        {
            EndpointGrouping.MultipleClientsFromFirstTagAndOperationName => ((IOperationNameGenerator)new MultipleClientsFromFirstTagAndOperationNameGenerator(), "{controller}" + ClientPostFix),
            EndpointGrouping.SingleClientFromOperationIdOperationName => (new SingleClientFromOperationIdOperationNameGenerator(), linqPadTypeName),
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
                Namespace = nameSpace,
                JsonLibrary = (CSharpJsonLibrary)jsonLibrary,
                ClassStyle = (CSharpClassStyle)classStyle,
            },
            UseHttpClientCreationMethod = false,
            DisposeHttpClient = true,
            GenerateSyncMethods = generateSyncMethods,
            GeneratePrepareRequestAndProcessResponseAsAsyncMethods = false
        };
        return settings;
    }
}
