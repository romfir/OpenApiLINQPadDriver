using System;
using System.Collections.Generic;
using System.Linq;
using static OpenApiLINQPadDriver.ParameterDescriptors;
namespace OpenApiLINQPadDriver;

public static class ClientGenerator
{
    public const string PrepareRequestFunctionName = "PrepareRequestFunction";

    public static string SingleClientFromOperationIdOperationNameGenerator(string nameSpace, string typeName)
        => GetClientPartial(nameSpace, typeName);

    public static string MultipleClientsFromOperationIdOperationNameGenerator(ICollection<string> clientTypeNames, string nameSpace, string typeName)
    {
        return $@"

namespace {nameSpace}
{{
    public partial class {typeName}
    {{
{GenerateFields()}

        /// <summary>
        /// This will set all <see cref=""{PrepareRequestFunctionName}""/> of nested clients
        /// </summary>
        public System.Action<{HttpClient.FullTypeName}, {HttpRequestMessage.FullTypeName}, string>? {PrepareRequestFunctionName} 
        {{
            set
            {{
{GeneratePrepareRequestFunctions()}
            }}
        }}


        public {typeName}({HttpClient.FullTypeName} {HttpClient.ParameterName})
        {{
{GenerateInitializations()}
        }}
    }}
}}

{string.Join(Environment.NewLine, clientTypeNames.Select(clientTypeName => GetClientPartial(nameSpace, clientTypeName)))}";

        string GenerateFields()
            => string.Join(Environment.NewLine,
                clientTypeNames.Select(clientTypeName =>
                    $"        public {clientTypeName} {clientTypeName} {{ get; }}"));

        string GeneratePrepareRequestFunctions()
            => string.Join(Environment.NewLine,
                clientTypeNames.Select(clientTypeName =>
                    $"                {clientTypeName}.{PrepareRequestFunctionName} = value;"));

        string GenerateInitializations()
            => string.Join(Environment.NewLine,
                clientTypeNames.Select(clientTypeName =>
                    $"            {clientTypeName} = new {clientTypeName}({HttpClient.ParameterName});"));
    }

    private static string GetClientPartial(string nameSpace, string typeName) 
        => $@"
namespace {nameSpace}
{{
    public partial class {typeName}
    {{
        public System.Action<{HttpClient.FullTypeName}, {HttpRequestMessage.FullTypeName}, string>? {PrepareRequestFunctionName} {{ get; set; }}

        partial void PrepareRequest({HttpClient.FullTypeName} client, {HttpRequestMessage.FullTypeName} request, string url)
        {{
            {PrepareRequestFunctionName}?.Invoke(client, request, url);
        }}
    }}
}}
";
}
