using System.Collections.Concurrent;
using ESCd.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ESCd.Extensions.Caching;

/// <summary> The default implementation of <see cref="IAsyncCache"/>. </summary>
/// <param name="cache"> The underlying <see cref="IMemoryCache"/> used to store values. </param>
/// <param name="options"> The options to be used to configure this instance. </param>
public sealed class AsyncCache(
    IMemoryCache cache,
    IOptions<AsyncCacheOptions> options ) : IAsyncCache, IDisposable
{
    private bool disposed;
    private readonly ConcurrentDictionary<CacheKey, AsyncCacheLock> locks = [];
    private readonly IOptions<AsyncCacheOptions> options = options;

    private async Task<IDisposable> AcquireLock( CacheKey key, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( key );

        return await locks.GetOrAdd( key, _ => new( _ ) )
            .Aquire( cancellation )
            .ConfigureAwait( false );
    }

    /// <inheritdoc />
    public void Dispose( )
    {
        if( !disposed )
        {
            foreach( var locker in locks.Values )
            {
                locker.Dispose();
            }

            locks.Clear();
            disposed = true;
        }

        GC.SuppressFinalize( this );
    }

    /// <inheritdoc />
    public async ValueTask<T?> GetOrCreateAsync<T>( CacheKey key, Func<ICacheEntryBuilder, CancellationToken, ValueTask<T>> factory, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( key );
        ArgumentNullException.ThrowIfNull( factory );
        ObjectDisposedException.ThrowIf( disposed, this );

        if( options.Value.IsDisabled )
        {
            using var entry = new CacheEntryBuilder( key );
            return await factory( entry, cancellation ).ConfigureAwait( false );
        }

        if( cache.TryGetValue<T>( key, out var value ) )
        {
            return value;
        }

        using( await AcquireLock( key, cancellation ).ConfigureAwait( false ) )
        {
            if( cache.TryGetValue( key, out value ) )
            {
                return value;
            }

            using var entry = new CacheEntryBuilder( key );

            value = await factory( entry, cancellation ).ConfigureAwait( false );
            if( !entry.IsPrevented )
            {
                return cache.Set( key, value, entry.ToOptions() );
            }

            return value;
        }
    }

    /// <inheritdoc />
    public void Remove( CacheKey key )
    {
        ArgumentNullException.ThrowIfNull( key );
        ObjectDisposedException.ThrowIf( disposed, this );

        cache.Remove( key );
        if( locks.TryRemove( key, out var locker ) )
        {
            locker.OnRemoved();
        }
    }

    /// <inheritdoc />
    public async ValueTask<T> SetAsync<T>( CacheKey key, T value, MemoryCacheEntryOptions? options, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( key );
        ObjectDisposedException.ThrowIf( disposed, this );

        if( this.options.Value.IsDisabled )
        {
            return value;
        }

        using( await AcquireLock( key, cancellation ).ConfigureAwait( false ) )
        {
            return cache.Set( key, value, options );
        }
    }

    private sealed class AsyncCacheLock( CacheKey key ) : IDisposable
    {
        private readonly SemaphoreSlim locker = new( 1, 1 );

        private int count;

        public int Count => count;
        public bool IsRemoved { get; private set; }
        public CacheKey Key { get; } = key;

        public async Task<AsyncCacheRelease> Aquire( CancellationToken cancellation )
        {
            Interlocked.Increment( ref count );
            try
            {
                await locker.WaitAsync( cancellation ).ConfigureAwait( false );
                return new( this );
            }
            catch( OperationCanceledException )
            {
                Interlocked.Decrement( ref count );
                throw;
            }
        }

        public void Dispose( ) => locker.Dispose();
        public void OnRemoved( )
        {
            IsRemoved = true;
            if( count is 0 )
            {
                Dispose();
            }
        }

        public void Release( )
        {
            locker.Release();
            if( Interlocked.Decrement( ref count ) is 0 && IsRemoved )
            {
                Dispose();
            }
        }
    }

    private readonly struct AsyncCacheRelease( AsyncCacheLock locker ) : IDisposable
    {
        public void Dispose( ) => locker.Release();
    }
}

sealed file class CacheEntryBuilder( CacheKey key ) : ICacheEntryBuilder
{
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    public IList<IChangeToken> ExpirationTokens { get; } = [];
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = [];
    public CacheItemPriority Priority { get; set; }
    public long? Size { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public object? Value { get; set; }

    public bool IsPrevented { get; private set; }
    public CacheKey Key { get; } = key;

    public void Dispose( )
    {
    }

    public ICacheEntryBuilder PreventCaching( bool prevent = true )
    {
        if( IsPrevented == prevent )
        {
            return this;
        }

        IsPrevented = prevent;
        return this;
    }

    public MemoryCacheEntryOptions ToOptions( )
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = AbsoluteExpiration,
            AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow,
            Priority = Priority,
            Size = Size,
            SlidingExpiration = SlidingExpiration,
        };

        foreach( var token in ExpirationTokens )
        {
            options.ExpirationTokens.Add( token );
        }

        foreach( var callback in PostEvictionCallbacks )
        {
            options.PostEvictionCallbacks.Add( callback );
        }

        return options;
    }
}