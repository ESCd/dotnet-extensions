using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace ESCd.Extensions.Http;

/// <summary> Extensions to <see cref="IServiceCollection"/> for adding Http related services. </summary>
public static class HttpServiceExtensions
{
    /// <summary> Adds an <see cref="ObjectPool{T}"/> of <see cref="QueryStringBuilder"/>s to the given <paramref name="services"/>. </summary>
    /// <param name="services"> The service collection. </param>
    public static IServiceCollection AddQueryStringBuilderObjectPool( this IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.TryAddSingleton(
            serviceProvider => serviceProvider.GetRequiredService<ObjectPoolProvider>()
                .CreateQueryStringBuilderPool() );

        return services;
    }
}