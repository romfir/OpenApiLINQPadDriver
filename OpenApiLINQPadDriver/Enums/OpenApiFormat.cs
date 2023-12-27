using System.ComponentModel;

namespace OpenApiLINQPadDriver.Enums;
public enum OpenApiFormat
{
    [Description(".yaml")]
    Yaml,
    [Description(".json")]
    Json,
}
