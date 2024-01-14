using System.Windows;

namespace OpenApiLINQPadDriver.Extensions;
internal static class FrameworkElementExtensions
{
    public static void UpdateSelectedValueProperty(this FrameworkElement element)
        => element.GetBindingExpression(System.Windows.Controls.Primitives.Selector.SelectedValueProperty)!.UpdateTarget();
}
