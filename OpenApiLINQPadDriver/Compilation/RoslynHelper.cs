using LINQPad.Extensibility.DataContext;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenApiLINQPadDriver.Compilation;
internal static class RoslynHelper
{
    private static readonly Dictionary<string, ReportDiagnostic> WarningOptions = new()
    {
        { "CS1591", ReportDiagnostic.Suppress } //Missing XML comment for publicly visible type or member 'Type_or_Member'
    };
    public static CompilationOutput CompileSource(CompilationInput input, bool buildInRelease, Action<string> logAction)
    {
        var optimization = buildInRelease ? OptimizationLevel.Release : OptimizationLevel.Debug;
        var csharpParseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        var syntaxTrees = input.SourceCode
            // https://forum.linqpad.net/discussion/3002/linqpad-seems-to-ignore-and-tags-in-documentation-comments#latest
            .Select(source => source.Replace("<remarks>", "<summary>").Replace("</remarks>", "</summary>"))
            .Select(source => CSharpSyntaxTree.ParseText(source, csharpParseOptions))
            .ToArray();

        logAction("Parsing code");

        var executableReferences = input.FilePathsToReference.Select(fileReference => MetadataReference.CreateFromFile(fileReference)).ToArray();

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(input.OutputPath);

        var options = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary,
            xmlReferenceResolver: XmlFileResolver.Default,
            nullableContextOptions: NullableContextOptions.Enable,
            optimizationLevel: optimization,
            metadataImportOptions: MetadataImportOptions.Public,
            specificDiagnosticOptions: WarningOptions
        );

        var csharpCompilation = CSharpCompilation.Create(fileNameWithoutExtension, syntaxTrees, executableReferences, options);
        var diagnostics = csharpCompilation.GetDiagnostics();
        var errorsFromCompilation = diagnostics.GetErrors();
        if (errorsFromCompilation.Length > 0)
        {
            var diagnosticsFromCompilation = diagnostics.GetWarnings();
            var compilationOutput = new CompilationOutput(errorsFromCompilation, diagnosticsFromCompilation);

            logAction("Getting initial diagnostics");

            return compilationOutput;
        }

        logAction("Getting initial diagnostics");

        var emitResult = csharpCompilation.Emit(input.OutputPath, xmlDocPath: GetXmlDocumentFileNameWithPath(input.OutputPath, fileNameWithoutExtension));

        logAction("Emitting dlls");

        var errors = emitResult.Diagnostics.GetErrors();
        var warnings = emitResult.Diagnostics.GetWarnings();

        logAction("Reading diagnostics from emit result");

        var output = new CompilationOutput(errors, warnings);

        return output;

        static string GetXmlDocumentFileNameWithPath(string outputPath, string fileNameWithoutExtension)
        {
            var dllDirectory = Path.GetDirectoryName(outputPath);

            return Path.Join(dllDirectory, fileNameWithoutExtension) + ".xml";
        }
    }
}
