using Microsoft.Extensions.Caching.Memory;

namespace ESCd.Extensions.Caching.Abstractions;

/// <summary> Represents the parameters for configuring an entry in an <see cref="IAsyncCache"/>. </summary>
public interface ICacheEntryBuilder : ICacheEntry
{
    /// <summary> Prevent the entry from being cached. </summary>
    public ICacheEntryBuilder PreventCaching( );
}