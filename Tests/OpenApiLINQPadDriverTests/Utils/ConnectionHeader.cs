using System.Globalization;
using System.Reflection;
using System.Security;

namespace OpenApiLINQPadDriverTests.Utils;
internal static class ConnectionHeader
{
    public static string Get<T>(string driverAssemblyName, string driverNamespace, T driverConfig, params string[] additionalNamespaces)
        where T : notnull 
        =>
        $"""
         <Query Kind="Program">
           
           <Connection>
             <NamingServiceVersion>2</NamingServiceVersion>
             <DisplayName>Test</DisplayName>
             <Driver Assembly="{driverAssemblyName}">{driverNamespace}</Driver>
             <DriverData>
         {string.Join(Environment.NewLine, GetKeyValues(driverConfig).Select(keyValuePair => $"      <{keyValuePair.Key}>{SecurityElement.Escape(keyValuePair.Value)}</{keyValuePair.Key}>"))}
             </DriverData>
           </Connection>
           <NuGetReference>FluentAssertions</NuGetReference>
         {string.Join(Environment.NewLine, new[] { "FluentAssertions" }.Concat(additionalNamespaces).Select(additionalNamespace => $"  <Namespace>{additionalNamespace}</Namespace>"))}
         </Query>
         """;

    private static IEnumerable<(string Key, string Value)> GetKeyValues<T>(T driverConfig)
        where T : notnull
    {
        return driverConfig.GetType().GetProperties()
            .Where(propertyInfo => propertyInfo is { CanRead: true, CanWrite: true })
            .Select(propertyInfo => (propertyInfo.Name, ValueToString(propertyInfo)));

        string ValueToString(PropertyInfo propertyInfo)
            => propertyInfo.GetValue(driverConfig) switch
            {
                null => string.Empty,
                bool v => v ? "true" : "false",
                IConvertible v => v.ToString(CultureInfo.InvariantCulture),
                _ => throw new NotSupportedException($"Could not convert {propertyInfo.Name} of type {propertyInfo.PropertyType} to string")
            };
    }
}

