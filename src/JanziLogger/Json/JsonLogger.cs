using System;

using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.ObjectPool;
namespace janzi.Logging.Json;
public sealed class JsonLogger : BackgroundWorkerLoggerBase
{
    private readonly Utf8JsonWriter _writer;
    private readonly ObjectPool<JsonLogEntry> poolProvider;

    public JsonLogger(
        Utf8JsonWriter writer, 
        string categoryName, 
        IExternalScopeProvider scopeProvider,
        ObjectPool<JsonLogEntry> poolProvider)
     :base(categoryName, scopeProvider)
    {
        _writer = writer;
        this.poolProvider = poolProvider;
    }
    protected override JsonLogEntry CreateLogEntry()
    {
        return poolProvider.Get();
    }

    protected override Task ProcessQueueItem(JsonLogEntry content)
    {
            JsonSerializer.Serialize(_writer, content, JsonLogEntryContext.Default.JsonLogEntry);
            poolProvider.Return(content);
            return Task.CompletedTask;
    }
}
