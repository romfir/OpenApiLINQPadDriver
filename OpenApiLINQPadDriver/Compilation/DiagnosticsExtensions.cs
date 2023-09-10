using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace OpenApiLINQPadDriver.Compilation;
internal static class DiagnosticsExtensions
{
    public static string[] GetErrors(this IEnumerable<Diagnostic> diagnostics)
        => diagnostics.Get(DiagnosticSeverity.Error);

    public static string[] GetWarnings(this IEnumerable<Diagnostic> diagnostics)
        => diagnostics.Get(DiagnosticSeverity.Warning);

    private static string[] Get(this IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity)
        => diagnostics.Where(d => d.Severity == severity).Select(e => e.ToString()).ToArray();
}
