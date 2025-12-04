using Microsoft.Extensions.ObjectPool;

namespace ESCd.Extensions.ObjectPool;

/// <summary> Extension methods for <see cref="ObjectPoolProvider"/>.</summary>
public static class ObjectPoolProviderExtensions
{
    /// <summary> Creates an <see cref="ObjectPool{T}"/> that pools <see cref="HashSet{T}"/> instances. </summary>
    /// <param name="provider"> The <see cref="ObjectPoolProvider"/>. </param>
    /// <param name="policy"> The policy to be used to configure pooling. </param>
    /// <returns> The <see cref="ObjectPool{T}"/>. </returns>
    public static ObjectPool<HashSet<T>> CreateHashSetPool<T>( this ObjectPoolProvider provider, HashSetPooledPolicy<T>? policy = default )
    {
        ArgumentNullException.ThrowIfNull( provider );
        return provider.Create( policy ?? new() );
    }
}