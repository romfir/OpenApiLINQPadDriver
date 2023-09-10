using LINQPad.Extensibility.DataContext;

namespace OpenApiLINQPadDriver;
internal static class ParameterDescriptors
{
    public static readonly ParameterDescriptor HttpClient = new("httpClient", $"{nameof(System)}.{nameof(System.Net)}.{nameof(System.Net.Http)}.{nameof(System.Net.Http.HttpClient)}");
    public static readonly ParameterDescriptor HttpRequestMessage = new("httpRequestMessage", $"{nameof(System)}.{nameof(System.Net)}.{nameof(System.Net.Http)}.{nameof(System.Net.Http.HttpRequestMessage)}");
}
