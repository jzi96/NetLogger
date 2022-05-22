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
    private readonly ObjectPool<JsonLogEntry> objectPoolProvider;

    public HttpJsonLogger(
        [NotNull] HttpJsonLoggerProvider provider,
        string categoryName,
        IExternalScopeProvider scopeProvider,
        ObjectPool<JsonLogEntry> objectPoolProvider)
        : base(provider, categoryName, scopeProvider)
    {
        this.objectPoolProvider = objectPoolProvider;
    }

    protected override JsonLogEntry CreateLogEntry()
    {
        return objectPoolProvider.Get();
    }
}
