using System;
using System.Linq;

namespace OpenApiLINQPadDriver.ValidationRules;

internal sealed class OpenApiUriValidationRule : RequiredUriValidationRule
{
    protected override string[] GetAllowedSchemes()
        => base.GetAllowedSchemes().Concat(new[] { Uri.UriSchemeFile }).ToArray();
}