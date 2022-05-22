using System.Text.Json;
namespace janzi.Logging.Json;

public record JsonFileLoggerOptions(string File, JsonWriterOptions WriterOptions);
