using System;
using System.Text.Json;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.Extensions.ObjectPool;

namespace janzi.Logging.Json;

[ProviderAlias("FileJson")]
public sealed class JsonFileLoggerProvider : ILoggerProvider
{
    private readonly LoggerExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
    private readonly ConcurrentDictionary<string, JsonLogger> _loggers = new ConcurrentDictionary<string, JsonLogger>(StringComparer.Ordinal);
    private readonly IOptions<JsonFileLoggerOptions> options;
    private readonly ObjectPool<JsonLogEntry> poolProvider;
    private readonly FileStream fileStream;
    private readonly Utf8JsonWriter utfStream;

    public JsonFileLoggerProvider(IOptions<JsonFileLoggerOptions> options, ObjectPool<JsonLogEntry> poolProvider)
    {
        this.options = options;
        this.poolProvider = poolProvider;
        if (string.IsNullOrEmpty(options.Value.File))
            throw new InvalidOperationException("A file must be configured for logging.");
        fileStream = new FileStream(options.Value.File, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096*4,true);
        utfStream = new Utf8JsonWriter(fileStream, options.Value.WriterOptions);
    }
    public ILogger CreateLogger(string categoryName)
    {
        if(fileStream.CanWrite)
            return _loggers.GetOrAdd(categoryName, category => new JsonLogger(this.utfStream, category, _scopeProvider, poolProvider));
        throw new Exception("File cannot be written!");
    }

    public void Dispose()
    {
        this.utfStream.Dispose();
        this.fileStream.Close();
        GC.SuppressFinalize(this);
    }
}