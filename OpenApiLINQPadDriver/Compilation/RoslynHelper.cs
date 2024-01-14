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
    public static CompilationOutput CompileSource(CompilationInput input, bool buildInRelease)
    {
        var optimization = buildInRelease ? OptimizationLevel.Release : OptimizationLevel.Debug;
        var syntaxTrees = input.SourceCode
            // https://forum.linqpad.net/discussion/3002/linqpad-seems-to-ignore-and-tags-in-documentation-comments#latest
            .Select(source => source.Replace("<remarks>", "<summary>").Replace("</remarks>", "</summary>"))
            .Select(source => CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose))).ToArray();

        var executableReferences = input.FilePathsToReference.Select(fileReference => MetadataReference.CreateFromFile(fileReference));

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
            var sourceCodeAroundFirstError = GetSurroundingSource(diagnostics.First(d => d.Severity == DiagnosticSeverity.Error).Location);
            var compilationOutput = new CompilationOutput(errorsFromCompilation, diagnosticsFromCompilation, input.FilePathsToReference, sourceCodeAroundFirstError);

            return compilationOutput;
        }

        var emitResult = csharpCompilation.Emit(input.OutputPath, xmlDocPath: GetXmlDocumentFileNameWithPath(input.OutputPath, fileNameWithoutExtension));
        var errors = emitResult.Diagnostics.GetErrors();
        var warnings = emitResult.Diagnostics.GetWarnings();

        var output = new CompilationOutput(errors, warnings, input.FilePathsToReference);

        return output;

        static string GetXmlDocumentFileNameWithPath(string outputPath, string fileNameWithoutExtension)
        {
            var dllDirectory = Path.GetDirectoryName(outputPath);

            return Path.Join(dllDirectory, fileNameWithoutExtension) + ".xml";
        }
    }

    private static string? GetSurroundingSource(Location? location)
    {
        if (location == null || !location.IsInSource)
            return null;

        var str = location.SourceTree.ToString();
        var mappedLineSpan = location.GetMappedLineSpan();
        var startLinePosition = mappedLineSpan.StartLinePosition;
        var line = startLinePosition.Line;
        if (line < 0)
            return null;
        var source = str.Split('\n');
        var count = Math.Max(0, line - 5);
        var num = Math.Min(source.Length - 1, line + 5);
        return string.Concat(source.Skip(count).Take(line - count).Select(l => l.Replace("\r", string.Empty) + "\r\n")) + "===>" + source[line].Replace("\r", string.Empty) + "<===" + string.Concat(source.Skip(line + 1).Take(num - line).Select(l => "\r\n" + l.Replace("\r", string.Empty)));
    }
}
