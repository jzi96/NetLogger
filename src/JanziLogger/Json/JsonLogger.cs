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
    private readonly ObjectPool<JsonLogEntry> poolProvider;

    public JsonLogger(
        BackgroundWorkerLoggerProvider provider, 
        string categoryName, 
        IExternalScopeProvider scopeProvider,
        ObjectPool<JsonLogEntry> poolProvider)
     :base(provider, categoryName, scopeProvider)
    {
        this.poolProvider = poolProvider;
    }
    protected override JsonLogEntry CreateLogEntry()
    {
        return poolProvider.Get();
    }
}
