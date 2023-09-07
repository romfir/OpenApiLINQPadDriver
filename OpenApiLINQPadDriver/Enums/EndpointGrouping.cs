using System.ComponentModel;

namespace OpenApiLINQPadDriver.Enums;

public enum EndpointGrouping
{
    [Description("Single client from OperationId and OperationName")]
    SingleClientFromOperationIdOperationName,

    [Description("Multiple clients from first tag and operationName")]
    MultipleClientsFromFirstTagAndOperationName
}
