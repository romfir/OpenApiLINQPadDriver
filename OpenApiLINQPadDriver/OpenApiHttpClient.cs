using System;
using System.Net.Http;

namespace OpenApiLINQPadDriver;

public sealed class OpenApiHttpClient : HttpClient
{
    private readonly ConfigurableHttpMessageHandler _handler;

    public OpenApiHttpClient() : this(new ConfigurableHttpMessageHandler())
    {
    }

    private OpenApiHttpClient(ConfigurableHttpMessageHandler handler) : base(handler, disposeHandler: true)
        => _handler = handler;

    // ReSharper disable once UnusedMember.Global
    public void ConfigureTransportOnce(Func<HttpMessageHandler> handlerFactory)
        => _handler.ConfigureOnce(handlerFactory);
}