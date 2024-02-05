namespace OpenApiLINQPadDriver.Compilation;
internal sealed class CompilationOutput
{
    public string[] Errors { get; }

    public string[] Warnings { get; }

    public CompilationOutput(string[] errors, string[] warnings)
    {
        Errors = errors;
        Warnings = warnings;
    }
}
