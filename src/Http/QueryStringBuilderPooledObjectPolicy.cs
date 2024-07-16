using Microsoft.Extensions.ObjectPool;

namespace ESCd.Extensions.Http;

/// <summary> Default <see cref="IPooledObjectPolicy{T}"/> for <see cref="QueryStringBuilder"/>s. </summary>
public sealed class QueryStringBuilderPooledObjectPolicy : IPooledObjectPolicy<QueryStringBuilder>
{
    /// <inheritdoc/>
    public QueryStringBuilder Create( ) => new();

    /// <inheritdoc/>
    public bool Return( QueryStringBuilder builder )
    {
        builder.Clear();
        return true;
    }
}