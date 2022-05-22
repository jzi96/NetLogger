using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace janzi.Logging;

public sealed class JsonLogEntry
{
    public DateTimeOffset Timestamp { get; set; }
    public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, object> Scope { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}

[JsonSourceGenerationOptions(WriteIndented = true,PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(JsonLogEntry))]
internal partial class JsonLogEntryContext : JsonSerializerContext
{
}
