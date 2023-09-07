using System.ComponentModel;

namespace OpenApiLINQPadDriver.Enums;

/// <summary>
/// Copy of <see cref="NJsonSchema.CodeGeneration.CSharp.CSharpClassStyle"/>
/// </summary>
public enum ClassStyle
{
    [Description("POCOs (Plain Old C# Objects)")]
    Poco,

    [Description("Classes implementing the INotifyPropertyChanged interface")]
    Inpc,

    [Description("Classes implementing the Prism base class")]
    Prism,

    [Description("Records - read only POCOs (Plain Old C# Objects)")]
    Record
}