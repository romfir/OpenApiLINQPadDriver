using LINQPad.Extensibility.DataContext;
using OpenApiLINQPadDriver.Enums;
using OpenApiLINQPadDriver.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenApiLINQPadDriver;
internal static class ReflectionSchemaBuilder
{
    public static IEnumerable<ExplorerItem> GenerateExplorerItems(Type contextType, EndpointGrouping endpointGrouping)
    {
        return endpointGrouping switch
        {
            EndpointGrouping.MultipleClientsFromFirstTagAndOperationName => GenerateForMultipleClients(contextType),
            EndpointGrouping.SingleClientFromOperationIdOperationName => GenerateSingleClient(contextType),
            _ => throw new ArgumentOutOfRangeException(nameof(endpointGrouping), endpointGrouping, null),
        };

        static IEnumerable<ExplorerItem> GenerateSingleClient(Type contextType)
        {
            var clientMethods = contextType.GetMethodsWithoutProperties();

            return GenerateFromMethods(clientMethods, static x => x);
        }

        static IEnumerable<ExplorerItem> GenerateForMultipleClients(Type contextType)
        {
            var contextProperties = contextType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .Where(p => p.Name != ClientGenerator.PrepareRequestFunctionName)
                .ToList();

            var items = new List<ExplorerItem>(contextProperties.Count);

            foreach (var client in contextProperties)
            {
                var clientType = client.PropertyType;

                var clientExplorerItem = new ExplorerItem(clientType.Name, ExplorerItemKind.Category, ExplorerIcon.Box);

                var clientMethods = clientType.GetMethodsWithoutProperties();

                clientExplorerItem.Children = GenerateFromMethods(clientMethods, methodName => clientType.Name + "." + methodName);

                items.Add(clientExplorerItem);
            }

            return items;
        }

        static List<ExplorerItem> GenerateFromMethods(MethodInfo[] methods, Func<string, string> dragTextGenerator)
        {
            var methodExplorerItems = new List<ExplorerItem>(methods.Length);

            foreach (var method in methods)
            {
                var methodExplorerItem = new ExplorerItem(method.Name, ExplorerItemKind.Schema, ExplorerIcon.StoredProc)
                {
                    DragText = dragTextGenerator(method.Name),
                };

                methodExplorerItems.Add(methodExplorerItem);
            }

            return methodExplorerItems;
        }
    }
}
