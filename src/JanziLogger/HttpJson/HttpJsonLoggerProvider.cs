using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ObjectPool;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace janzi.Logging.HttpJson;
[ProviderAlias("HttpJson")]
public sealed class HttpJsonLoggerProvider : BackgroundWorkerLoggerProvider, ILoggerProvider
{
    private readonly ConcurrentDictionary<string, HttpJsonLogger> _loggers = new ConcurrentDictionary<string, HttpJsonLogger>(StringComparer.Ordinal);
    private readonly LoggerExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    private readonly HttpJsonLoggingOptions options;
    private readonly IHttpClientFactory httpFactory;
    private readonly ObjectPool<JsonLogEntry> poolProvider;

    public HttpJsonLoggerProvider(HttpJsonLoggingOptions options, IHttpClientFactory httpFactory, ObjectPool<JsonLogEntry> poolProvider)
    {
        this.options = options;
        this.httpFactory = httpFactory;
        this.poolProvider = poolProvider;
    }

    public IHttpClientFactory HttpFactory => httpFactory;

    public HttpJsonLoggingOptions Options => options;

    public override ILogger CreateLogger(string categoryName)
    {
        if(Disposed)
            throw new ObjectDisposedException(nameof(HttpJsonLogger));
        return _loggers.GetOrAdd(categoryName, category => new HttpJsonLogger(this, categoryName, _scopeProvider, poolProvider));
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        var b = _loggers.ToArray();
        _loggers.Clear();
        for (int i = 0; i < b.Length; i++)
        {
            b[i].Value.Dispose();
        }
    }
    protected override async Task ProcessQueueItem(JsonLogEntry content)
    {
        try
        {
            HttpClient? client;
            if (string.IsNullOrEmpty(Options.HttpClientName))
                client = HttpFactory.CreateClient();
            else
                client = HttpFactory.CreateClient(Options.HttpClientName);
            var message = JsonContent.Create<JsonLogEntry>(content);

            HttpRequestMessage req = new HttpRequestMessage(Options.Method, Options.ShipTo);
            req.Content = message;
            await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine("Failed to process messages: " + ex.ToString(), nameof(HttpJsonLogger));
        }
        finally
        {
            poolProvider.Return(content);
        }
    }
}
