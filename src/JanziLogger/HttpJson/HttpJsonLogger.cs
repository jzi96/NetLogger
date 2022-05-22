using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using janzi.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace janzi.Logging.HttpJson;
public sealed class HttpJsonLogger : BackgroundWorkerLoggerBase
{
    private readonly HttpJsonLoggerProvider provider;
    private readonly ObjectPool<JsonLogEntry> objectPoolProvider;

    public HttpJsonLogger(
        [NotNull] HttpJsonLoggerProvider provider,
        string categoryName,
        IExternalScopeProvider scopeProvider,
        ObjectPool<JsonLogEntry> objectPoolProvider)
        : base(categoryName, scopeProvider)
    {
        this.provider = provider;
        this.objectPoolProvider = objectPoolProvider;
    }

    protected override JsonLogEntry CreateLogEntry()
    {
        return objectPoolProvider.Get();
    }

    protected override async Task ProcessQueueItem(JsonLogEntry content)
    {
        try
        {
            HttpClient? client;
            if (string.IsNullOrEmpty(provider.Options.HttpClientName))
                client = provider.HttpFactory.CreateClient();
            else
                client = provider.HttpFactory.CreateClient(provider.Options.HttpClientName);
            var message = JsonContent.Create<JsonLogEntry>(content);

            HttpRequestMessage req = new HttpRequestMessage(provider.Options.Method, provider.Options.ShipTo);
            req.Content = message;
            await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine("Failed to process messages: " + ex.ToString(), nameof(HttpJsonLogger));
        }
        finally
        {
            objectPoolProvider.Return(content);
        }
    }
}
