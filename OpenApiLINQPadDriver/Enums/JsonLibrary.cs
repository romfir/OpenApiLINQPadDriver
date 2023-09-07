using System.ComponentModel;

namespace OpenApiLINQPadDriver.Enums;

/// <summary>
/// Copy of <see cref="NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary"/>
/// </summary>
public enum JsonLibrary
{
    [Description("Newtonsoft.Json")]
    NewtonsoftJson,

    [Description("System.Text.Json")]
    SystemTextJson
}
