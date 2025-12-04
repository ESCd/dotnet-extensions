using Microsoft.Extensions.ObjectPool;

namespace ESCd.Extensions.ObjectPool;

/// <summary> A policy for pooling <see cref="HashSet{T}"/> instances. </summary>
public sealed class HashSetPooledPolicy<T> : PooledObjectPolicy<HashSet<T>>
{
    /// <summary> The <see cref="EqualityComparer{T}"/> to initialize <see cref="HashSet{T}"/> instances with. </summary>
    public IEqualityComparer<T> Comparer { get; set; } = EqualityComparer<T>.Default;

    /// <summary> Gets or sets the initial capacity of pooled <see cref="HashSet{T}"/> instances. </summary>
    /// <value>Defaults to <c>100</c>.</value>
    public int InitialCapacity { get; set; } = 100;

    /// <summary> Gets or sets the maximum value for <see cref="HashSet{T}.Capacity"/> that is allowed to be retained, when <see cref="Return(HashSet{T})"/> is invoked. </summary>
    /// <value>Defaults to <c>4096</c>.</value>
    public int MaximumRetainedCapacity { get; set; } = 4 * 1024;

    /// <inheritdoc />
    public override HashSet<T> Create( ) => new( InitialCapacity, Comparer );

    /// <inheritdoc />
    public override bool Return( HashSet<T> value )
    {
        ArgumentNullException.ThrowIfNull( value );

        if( value.Capacity > MaximumRetainedCapacity )
        {
            // Too big. Discard this one.
            return false;
        }

        value.Clear();
        return true;
    }
}

