using Microsoft.Extensions.ObjectPool;

namespace ESCd.Extensions.Http;

/// <summary> Extensions to <see cref="ObjectPoolProvider"/>. </summary>
public static class ObjectPoolProviderExtensions
{
    /// <summary> Create an <see cref="ObjectPool{T}"/> of <see cref="QueryStringBuilder"/>s. </summary>
    /// <param name="provider"> The provider used to create the pool. </param>
    public static ObjectPool<QueryStringBuilder> CreateQueryStringBuilderPool( this ObjectPoolProvider provider )
    {
        ArgumentNullException.ThrowIfNull( provider );
        return provider.Create( new PooledQueryStringBuilderPolicy() );
    }

    /// <summary> Create an <see cref="ObjectPool{T}"/> of <see cref="QueryStringBuilder"/>s. </summary>
    /// <param name="provider"> The provider used to create the pool. </param>
    /// <param name="initialCapacity"> The initial capacity of pooled instances. </param>
    /// <param name="maximumCapacity"> The maxmimum capacity of pooled instances. </param>
    public static ObjectPool<QueryStringBuilder> CreateQueryStringBuilderPool( this ObjectPoolProvider provider, int initialCapacity, int maximumCapacity )
    {
        ArgumentNullException.ThrowIfNull( provider );
        return provider.Create( new PooledQueryStringBuilderPolicy
        {
            InitialCapacity = initialCapacity,
            MaximumRetainedCapacity = maximumCapacity,
        } );
    }
}

/// <summary> A policy for pooling <see cref="QueryStringBuilder"/>s. </summary>
public sealed class PooledQueryStringBuilderPolicy : PooledObjectPolicy<QueryStringBuilder>
{
    /// <summary> The initial capacity of pooled instances. </summary>
    public int InitialCapacity { get; set; } = sizeof( char ) * 128;

    /// <summary> The maximum capacity of pooled instances. </summary>
    public int MaximumRetainedCapacity { get; set; } = sizeof( char ) * 2048;

    /// <inheritdoc/>
    public override QueryStringBuilder Create( ) => new( InitialCapacity );

    /// <inheritdoc/>
    public override bool Return( QueryStringBuilder builder )
    {
        if( builder.Capacity > MaximumRetainedCapacity )
        {
            return false;
        }

        builder.Clear();
        return true;
    }
}