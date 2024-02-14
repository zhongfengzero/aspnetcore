// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Caching.Distributed;

public static class ReadThroughCacheServiceExtensions
{
    public static IServiceCollection AddReadThroughCache(this IServiceCollection services, Action<ReadThroughCacheOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(setupAction);
        AddReadThroughCache(services);
        services.Configure(setupAction);
        return services;
    }

    public static IServiceCollection AddReadThroughCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache(); // we need a backend; use in-proc by default
        services.AddSingleton<IReadThroughCacheSerializerFactory, DefaultJsonSerializerFactory>();
        services.AddSingleton<IReadThroughCacheSerializer<string>, StringSerializer>();
        services.AddSingleton(typeof(IReadThroughCache), typeof(ReadThroughCache));
        return services;
    }
}