using ESCd.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ESCd.Extensions.Caching.Tests;

public sealed class AsyncCacheTests
{
    [Fact( DisplayName = "IsDisabled: does not cache" )]
    public async Task IsDisabled_DoesNot_Cache( )
    {
        using var cache = new AsyncCache(
            new MemoryCache( Options.Create<MemoryCacheOptions>( new() ) ),
            Options.Create<AsyncCacheOptions>( new()
            {
                IsDisabled = true
            } ) );

        Assert.NotEqual( await Cache(), await Cache() );

        ValueTask<Guid> Cache( ) => cache.GetOrCreateAsync(
            new( nameof( IsDisabled_DoesNot_Cache ) ),
            ( _, _ ) => ValueTask.FromResult( Guid.NewGuid() ),
            default );
    }

    [Fact( DisplayName = "PreventCaching: does not cache" )]
    public async Task PreventCaching_DoesNot_Cache( )
    {
        using var cache = new AsyncCache(
            new MemoryCache( Options.Create<MemoryCacheOptions>( new() ) ),
            Options.Create<AsyncCacheOptions>( new() ) );

        Assert.NotEqual( await Cache(), await Cache() );

        ValueTask<Guid> Cache( ) => cache.GetOrCreateAsync(
            new( nameof( IsDisabled_DoesNot_Cache ) ),
            ( entry, _ ) =>
            {
                entry.PreventCaching();
                return ValueTask.FromResult( Guid.NewGuid() );
            },
            default );
    }

    [Fact( DisplayName = "GetOrCreateAsync: does lock concurrent access" )]
    public async Task GetOrCreateAsync_Does_LockConcurrentAccess( )
    {
        using var cache = new AsyncCache(
            new MemoryCache( Options.Create<MemoryCacheOptions>( new() ) ),
            Options.Create<AsyncCacheOptions>( new() ) );

        var concurrency = 0;
        var invocations = 0;

        var results = await Task.WhenAll(
            cache.GetOrCreateAsync( new( nameof( GetOrCreateAsync_Does_LockConcurrentAccess ) ), Factory, default ).AsTask(),
            cache.GetOrCreateAsync( new( nameof( GetOrCreateAsync_Does_LockConcurrentAccess ) ), Factory, default ).AsTask(),
            cache.GetOrCreateAsync( new( nameof( GetOrCreateAsync_Does_LockConcurrentAccess ) ), Factory, default ).AsTask() );

        var result = results[ 0 ];
        foreach( var value in results )
        {
            Assert.Equal( result, value );
        }

        Assert.Equal( 1, concurrency );

        async ValueTask<Guid> Factory( ICacheEntryBuilder entry, CancellationToken cancellation )
        {
            ArgumentNullException.ThrowIfNull( entry );

            var current = Interlocked.Increment( ref invocations );
            InterlockedMax( ref concurrency, current );

            await Task.Delay( 125, cancellation );
            return Guid.NewGuid();
        }

        static void InterlockedMax( ref int location, int value )
        {
            int initial, computed;
            do
            {
                initial = Volatile.Read( ref location );
                computed = Math.Max( initial, value );
            }
            while( initial != Interlocked.CompareExchange( ref location, computed, initial ) );
        }
    }

    [Fact( DisplayName = "GetOrCreateAsync: throws cancellation" )]
    public async Task GetOrCreateAsync_DoesThrow_Cancellation( )
    {
        using var cache = new AsyncCache(
            new MemoryCache( Options.Create<MemoryCacheOptions>( new() ) ),
            Options.Create<AsyncCacheOptions>( new() ) );

        using var cancellation = new CancellationTokenSource();

        var task1 = cache.GetOrCreateAsync(
            new( nameof( GetOrCreateAsync_DoesThrow_Cancellation ) ),
            async ( _, cancellation ) =>
            {
                await Task.Delay( Timeout.Infinite, cancellation );
                return -1;
            },
            cancellation.Token ).AsTask();

        // NOTE: yield to allow the first task to acquire a lock
        await Task.Yield();

        var task2 = cache.GetOrCreateAsync(
            new( nameof( GetOrCreateAsync_DoesThrow_Cancellation ) ),
            ( _, cancellation ) => ValueTask.FromResult( 1 ),
            default ).AsTask();

        cancellation.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>( ( ) => task1 );

        Assert.Equal( 1, await task2 );
    }

    [Fact( DisplayName = "GetOrCreateAsync: throws object disposed" )]
    public async Task GetOrCreateAsync_DoesThrow_ObjectDisposed( )
    {
        var cache = new AsyncCache(
            new MemoryCache( Options.Create<MemoryCacheOptions>( new() ) ),
            Options.Create<AsyncCacheOptions>( new() ) );

        cache.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(
            async ( ) => await cache.GetOrCreateAsync(
                new( nameof( GetOrCreateAsync_DoesThrow_ObjectDisposed ) ),
                ( _, _ ) => ValueTask.FromResult<object?>( default ),
                default ) );
    }

    [Fact( DisplayName = "Remove: throws object disposed" )]
    public void Remove_DoesThrow_ObjectDisposed( )
    {
        var cache = new AsyncCache(
            new MemoryCache( Options.Create<MemoryCacheOptions>( new() ) ),
            Options.Create<AsyncCacheOptions>( new() ) );

        cache.Dispose();
        Assert.Throws<ObjectDisposedException>( ( ) => cache.Remove( new( nameof( GetOrCreateAsync_DoesThrow_ObjectDisposed ) ) ) );
    }

    [Fact( DisplayName = "SetAsync: throws object disposed" )]
    public async Task SetAsync_DoesThrow_ObjectDisposed( )
    {
        var cache = new AsyncCache(
            new MemoryCache( Options.Create<MemoryCacheOptions>( new() ) ),
            Options.Create<AsyncCacheOptions>( new() ) );

        cache.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(
            async ( ) => await cache.SetAsync<object?>(
                new( nameof( GetOrCreateAsync_DoesThrow_ObjectDisposed ) ),
                default,
                default,
                default ) );
    }
}