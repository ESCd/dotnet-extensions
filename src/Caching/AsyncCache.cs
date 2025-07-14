using System.Collections.Concurrent;
using ESCd.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace ESCd.Extensions.Caching;

internal sealed class AsyncCache( IMemoryCache cache ) : IAsyncCache, IDisposable
{
    private bool disposed;
    private readonly ConcurrentDictionary<CacheKey, AsyncCacheLock> locks = [];

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

    public async ValueTask<T?> GetOrCreateAsync<T>( CacheKey key, Func<ICacheEntryBuilder, CancellationToken, ValueTask<T>> factory, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( key );
        ArgumentNullException.ThrowIfNull( factory );
        ObjectDisposedException.ThrowIf( disposed, this );

        if( cache.TryGetValue<T>( key, out var value ) )
        {
            return value;
        }

        using( await locks.GetOrAdd( key, _ => new() ).Aquire( cancellation ).ConfigureAwait( false ) )
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

    public async ValueTask<T> SetAsync<T>( CacheKey key, T value, MemoryCacheEntryOptions options, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( key );

        using( await locks.GetOrAdd( key, _ => new() ).Aquire( cancellation ).ConfigureAwait( false ) )
        {
            cache.Set( key, value, options );
            return value;
        }
    }

    private sealed class AsyncCacheLock : IDisposable
    {
        private readonly SemaphoreSlim locker = new( 1, 1 );

        private int count;

        public int Count => count;
        public bool IsRemoved { get; private set; }

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
    public object Key { get; } = key;
    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = [];
    public CacheItemPriority Priority { get; set; }
    public long? Size { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public object? Value { get; set; }

    public bool IsPrevented { get; private set; }

    public void Dispose( )
    {
    }

    public ICacheEntryBuilder PreventCaching( )
    {
        if( IsPrevented )
        {
            return this;
        }

        IsPrevented = true;
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