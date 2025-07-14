using ESCd.Extensions.Caching.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ESCd.Extensions.Caching;

/// <summary> Provides extension methods for configuring an <see cref="IAsyncCache"/> service. </summary>
public static class AsyncCacheServiceExtensions
{
    /// <summary> Adds <see cref="IAsyncCache"/> to the given <paramref name="services"/>. </summary>
    /// <param name="services"> The service collection to register services. </param>
    public static IServiceCollection AddAsyncCache( this IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        services.AddMemoryCache()
            .TryAddSingleton<IAsyncCache, AsyncCache>();

        return services;
    }
}