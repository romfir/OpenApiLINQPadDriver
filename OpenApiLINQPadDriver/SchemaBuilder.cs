using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using NJsonSchema;
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
        var jsonLibrary = driverProperties.JsonLibrary;
        var settings = CreateCsharpClientGeneratorSettings(endpointGrouping, jsonLibrary, classStyle, driverProperties.GenerateSyncMethods, mainContextType);

        var generator = new CSharpClientGenerator(document, settings);

        var codeGeneratedByNSwag = generator.GenerateFile();

        MeasureTimeAndAddTimeExecutionExplorerItem("Generating NSwag classes");

        //possibly this switch should be an if based on SupportsMultipleClients?
        var clientSourceCode = endpointGrouping switch
        {
            EndpointGrouping.SingleClientFromOperationIdOperationName => ClientGenerator.SingleClientFromOperationIdOperationNameGenerator(mainContextType),
            EndpointGrouping.MultipleClientsFromFirstTagAndOperationName => ClientGenerator.MultipleClientsFromOperationIdOperationNameGenerator(GetClientNames(), mainContextType),
            _ => throw new InvalidOperationException()
        };

        MeasureTimeAndAddTimeExecutionExplorerItem("Generating clients partials");

        var references = driverProperties.GetCoreFxReferenceAssemblies()
            .AppendIf(jsonLibrary == JsonLibrary.NewtonsoftJson, typeof(JsonConvert).Assembly.Location)
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
        }, driverProperties.BuildInRelease, MeasureTimeAndAddTimeExecutionExplorerItem);

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
           // File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "log.txt"), name + " " + elapsed + Environment.NewLine);
            timeExplorerItem.Children.Add(ExplorerItemHelper.CreateForElapsedTime(name, elapsed));
        }
    }

    private static CustomCSharpClientGeneratorSettings CreateCsharpClientGeneratorSettings(EndpointGrouping endpointGrouping, JsonLibrary jsonLibrary, ClassStyle classStyle, bool generateSyncMethods,
        TypeDescriptor type)
    {
        var (operationNameGenerator, className) = endpointGrouping switch
        {
            EndpointGrouping.MultipleClientsFromFirstTagAndOperationName
                => ((IOperationNameGenerator)new MultipleClientsFromFirstTagAndOperationNameGenerator(), "{controller}" + ClientPostFix),

            EndpointGrouping.SingleClientFromOperationIdOperationName
                => (new SingleClientFromOperationIdOperationNameGenerator(), type.Name),
            _ => throw new InvalidOperationException()
        };

        var settings = new CustomCSharpClientGeneratorSettings
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
            GeneratePrepareRequestAndProcessResponseAsAsyncMethods = false
        };
        return settings;
    }

    private sealed class CustomCSharpClientGeneratorSettings : CSharpClientGeneratorSettings
    {
        public override string GenerateControllerName(string controllerName)
        {
            var convertedToUpperCaseUsingNSwagLib = ConversionUtilities.ConvertToUpperCamelCase(controllerName, false);
            var escapedToBeValidIdentifier = EscapeCSharpIdentifier(convertedToUpperCaseUsingNSwagLib);
            return ClassName.Replace("{controller}", escapedToBeValidIdentifier);
        }

        //https://github.com/icsharpcode/CodeConverter/blob/3284c3d228040fc4d0ea9c5a05129b1b2c0fc858/CodeConverter/CSharp/CommonConversions.cs#L342
        private static string EscapeCSharpIdentifier(string text)
        {
            if(string.IsNullOrEmpty(text))
                return text;

            if (!SyntaxFacts.IsValidIdentifier(text))
            {
                text = new string(text.Where(SyntaxFacts.IsIdentifierPartCharacter).ToArray());
                if (!SyntaxFacts.IsIdentifierStartCharacter(text[0]))
                    text = "a" + text;
            }

            //not needed because we concat this name with 'Client' postfix, but if we end up ditching it, it may be useful
            if (SyntaxFacts.GetKeywordKind(text) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(text) != SyntaxKind.None)
                text = "@" + text;

            return text;
        }
    }
}
