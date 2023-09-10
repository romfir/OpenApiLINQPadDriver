using System;
using System.Reflection;

namespace OpenApiLINQPadDriver.Extensions;
internal static class AssemblyExtensions
{
    public static Type GetType(this Assembly assembly, TypeDescriptor type)
        => assembly.GetType(type.ToString(), true)!;
}
