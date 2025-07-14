namespace ESCd.Extensions.Caching.Abstractions;

/// <summary> Represents the options for configuring an <see cref="IAsyncCache"/>. </summary>
public sealed class AsyncCacheOptions
{
    /// <summary> Whether caching should be disabled. </summary>
    public bool IsDisabled { get; set; }
}