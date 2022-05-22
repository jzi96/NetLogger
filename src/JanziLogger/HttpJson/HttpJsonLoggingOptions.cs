using System.Net.Http;

namespace janzi.Logging.HttpJson;

public record HttpJsonLoggingOptions(string? HttpClientName, string ShipTo, HttpMethod Method);
