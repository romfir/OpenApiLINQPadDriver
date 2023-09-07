using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace OpenApiLINQPadDriver.ValidationRules;
internal abstract class RequiredUriValidationRule : ValidationRule
{
    protected virtual string[] GetAllowedSchemes() => new[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not string userInput || string.IsNullOrWhiteSpace(userInput))
            return new ValidationResult(false, "Required");

        try
        {
            var uri = new Uri(userInput);

            var scheme = uri.Scheme;
            var allowedSchemes = GetAllowedSchemes();
            if (!allowedSchemes.Contains(scheme))
            {
                return new ValidationResult(false, $"\"{scheme}\" scheme is not allowed (Allowed schemes: {string.Join(", ", allowedSchemes)}");
            }
        }
        catch (Exception e)
        {
            return new ValidationResult(false, e.Message);
        }

        return ValidationResult.ValidResult;
    }
}
