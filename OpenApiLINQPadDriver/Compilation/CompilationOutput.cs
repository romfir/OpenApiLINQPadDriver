namespace OpenApiLINQPadDriver.Compilation;
internal sealed class CompilationOutput
{
    public string[] References { get; }

    public bool Successful { get; }

    public string[] Errors { get; }

    public string[] Warnings { get; }

    public string? SourceCodeAroundFirstError { get; }

    public CompilationOutput(string[] errors, string[] warnings, string[] references, string? sourceCodeAroundFirstError = null)
    {
        Successful = errors.Length == 0;
        Errors = errors;
        Warnings = warnings;
        References = references;
        SourceCodeAroundFirstError = sourceCodeAroundFirstError;
    }
}
