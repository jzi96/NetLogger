using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace janzi.Logging.HttpJson;

public static class HttpJsonLoggerProviderExtensions
{
    public static ILoggingBuilder AddHttpJsonLogging(this ILoggingBuilder builder, Action<HttpJsonLoggingOptions> configure)
    {
        builder.Services.AddSingleton<ILoggerProvider, HttpJsonLoggerProvider>();
        builder.Services.Configure(configure);
        return builder;
    }
    public static ILoggingBuilder AddHttpJsonLogging(this ILoggingBuilder builder, IConfiguration configuration)
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
        builder.Services.AddSingleton<ILoggerProvider, HttpJsonLoggerProvider>();
        builder.Services.AddOptions<HttpJsonLoggingOptions>("Logging:HttpJson");
        return builder;
    }
}