using Microsoft.Extensions.Caching.Memory;

namespace ESCd.Extensions.Caching.Abstractions;

/// <summary> Represents a local in-memory cache whose values are not serialized, and provides locking around mutations. </summary>
public interface IAsyncCache
{
    /// <summary> Get or Create an entry. </summary>
    /// <typeparam name="T"> The type of the value stored in the entry. </typeparam>
    /// <param name="key"> The entry's key. </param>
    /// <param name="factory"> A method that returns the value to be cached, if an entry doesn't exist. </param>
    /// <param name="cancellation"> A token that cancels cache access. </param>
    public ValueTask<T?> GetOrCreateAsync<T>( CacheKey key, Func<ICacheEntryBuilder, CancellationToken, ValueTask<T>> factory, CancellationToken cancellation );

    /// <summary> Remove an entry. </summary>
    /// <param name="key"> The entry's key. </param>
    public void Remove( CacheKey key );

    /// <summary> Set an entry to a given <paramref name="value"/>. </summary>
    /// <typeparam name="T"> The type of the <paramref name="value"/>. </typeparam>
    /// <param name="key"> The entry's key. </param>
    /// <param name="value"> The value to be stored in the entry. </param>
    /// <param name="options"> The options to be used to create the entry. </param>
    /// <param name="cancellation"> A token that cancels cache access. </param>
    public ValueTask<T> SetAsync<T>( CacheKey key, T value, MemoryCacheEntryOptions options, CancellationToken cancellation );
}