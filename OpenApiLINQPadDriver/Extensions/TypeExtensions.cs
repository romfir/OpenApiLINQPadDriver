using System;
using System.Linq;
using System.Reflection;

namespace OpenApiLINQPadDriver.Extensions;

internal static class TypeExtensions
{
    public static MethodInfo[] GetMethodsWithoutProperties(this Type type, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        => type.GetMethods(bindingAttr)
            .Where(m => !m.IsSpecialName)
            .ToArray();
}
