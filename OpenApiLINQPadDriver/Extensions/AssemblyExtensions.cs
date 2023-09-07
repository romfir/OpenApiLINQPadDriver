using System;
using System.Reflection;

namespace OpenApiLINQPadDriver.Extensions;

internal static class AssemblyExtensions
{
    public static Type GetType(this Assembly assembly, string nameSpace, string typeName)
        => assembly.GetType(nameSpace + "." + typeName, true)!;
}
