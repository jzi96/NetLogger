using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace janzi.Logging.Json;

public static class JsonFileLoggerProviderExtensions
{
    public static ILoggingBuilder AddFileJsonLogging(this ILoggingBuilder builder, Action<JsonFileLoggerOptions> configure)
    {
        builder.Services.AddSingleton<ILoggerProvider, JsonFileLoggerProvider>();
        builder.Services.Configure(configure);
        return builder;
    }
    public static ILoggingBuilder AddFileJsonLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        // var section = configuration.GetSection("Logging").GetSection("HttpJson");
        // HttpJsonLoggingOptions opt = new HttpJsonLoggingOptions();
        // section.Bind(opt);
        builder.Services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        builder.Services.TryAddSingleton<ObjectPool<JsonLogEntry>>(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new JsonLogEntryPooledObjectPolicy();
            return provider.Create(policy);
        });
        builder.Services.AddSingleton<ILoggerProvider, JsonFileLoggerProvider>();
        builder.Services.AddOptions<JsonFileLoggerProvider>("Logging:FileJson");
        return builder;
    }
}
