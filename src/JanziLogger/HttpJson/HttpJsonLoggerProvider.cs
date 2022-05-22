using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ObjectPool;

namespace janzi.Logging.HttpJson;
[ProviderAlias("HttpJson")]
public sealed class HttpJsonLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, HttpJsonLogger> _loggers = new ConcurrentDictionary<string, HttpJsonLogger>(StringComparer.Ordinal);
    private readonly LoggerExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    private readonly HttpJsonLoggingOptions options;
    private readonly IHttpClientFactory httpFactory;
    private readonly ObjectPool<JsonLogEntry> poolProvider;
    private bool isDisposed;

    public HttpJsonLoggerProvider(HttpJsonLoggingOptions options, IHttpClientFactory httpFactory, ObjectPool<JsonLogEntry> poolProvider)
    {
        this.options = options;
        this.httpFactory = httpFactory;
        this.poolProvider = poolProvider;
    }

    public IHttpClientFactory HttpFactory => httpFactory;

    public HttpJsonLoggingOptions Options => options;

    public ILogger CreateLogger(string categoryName)
    {
        if(isDisposed)
            throw new ObjectDisposedException(nameof(HttpJsonLogger));
        return _loggers.GetOrAdd(categoryName, category => new HttpJsonLogger(this, categoryName, _scopeProvider, poolProvider));
    }

    public void Dispose()
    {
        isDisposed = true;
        var b = _loggers.ToArray();
        _loggers.Clear();
        for (int i = 0; i < b.Length; i++)
        {
            b[i].Value.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
