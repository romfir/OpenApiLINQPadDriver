using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApiLINQPadDriver;

internal sealed class ConfigurableHttpMessageHandler : HttpMessageHandler
{
#if NET9_0_OR_GREATER
    private readonly Lock _sync = new();
#else
    private readonly object _sync = new();
#endif

    private Func<HttpMessageHandler>? _factory;
    private HttpMessageInvoker? _invoker;
    private bool _disposed;

    public void ConfigureOnce(Func<HttpMessageHandler> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        lock (_sync)
        {
#if NET7_0_OR_GREATER
            ObjectDisposedException.ThrowIf(_disposed, this);
#else
            ThrowIfDisposed();
#endif

            // The transport has already started.
            if (_invoker is not null)
                return;

            // First configuration wins.
            _factory ??= factory;
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => GetInvoker().SendAsync(request, cancellationToken);

    private HttpMessageInvoker GetInvoker()
    {
        lock (_sync)
        {
#if NET7_0_OR_GREATER
            ObjectDisposedException.ThrowIf(_disposed, this);
#else
            ThrowIfDisposed();
#endif

            if (_invoker is not null)
                return _invoker;

            var handler = (_factory ?? CreateDefaultHandler)();

            _invoker = new HttpMessageInvoker(
                handler,
                disposeHandler: true);

            return _invoker;
        }
    }

    private static HttpMessageHandler CreateDefaultHandler()
        => new HttpClientHandler();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_sync)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _invoker?.Dispose();
                }
            }
        }

        base.Dispose(disposing);
    }

#if !NET7_0_OR_GREATER
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
#endif
}