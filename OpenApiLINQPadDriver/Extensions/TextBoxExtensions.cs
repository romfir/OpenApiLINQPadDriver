using System.Windows.Controls;

namespace OpenApiLINQPadDriver.Extensions;
internal static class TextBoxExtensions
{
    public static void UpdateText(this TextBox texBox) 
        => texBox.GetBindingExpression(TextBox.TextProperty)!.UpdateTarget();
}
