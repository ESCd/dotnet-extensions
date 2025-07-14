using System.Collections.Immutable;
using System.ComponentModel;

namespace ESCd.Extensions.Caching.Abstractions;

/// <summary> Represents the key to an entry in an <see cref="IAsyncCache"/>. </summary>
[ImmutableObject( true )]
public sealed class CacheKey( params ImmutableArray<string> parts ) : IEquatable<CacheKey>
{
    private const char Delimiter = '|';

    /// <summary> The string parts that the key is composed of. </summary>
    public ImmutableArray<string> Parts { get; } = parts;

    /// <summary> Create a key, combining an existing <paramref name="key"/> with the given <paramref name="parts"/>. </summary>
    /// <param name="key"> The parent/prefix cache. </param>
    /// <param name="parts"> The parts to be combined with <paramref name="key"/>. </param>
    public CacheKey( CacheKey key, params ImmutableArray<string> parts ) : this( [ .. key.Parts, .. parts ] )
    {
    }

    /// <inheritdoc />
    public bool Equals( CacheKey? other ) => other?.Parts.SequenceEqual( Parts, StringComparer.OrdinalIgnoreCase ) is true;

    /// <inheritdoc />
    public override bool Equals( object? value ) => value is CacheKey key && Equals( key );

    /// <inheritdoc />
    public override int GetHashCode( )
    {
        var hash = new HashCode();
        foreach( var key in Parts )
        {
            hash.Add( key );
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString( ) => string.Join( Delimiter, Parts );

    /// <inheritdoc />
    public static bool operator ==( CacheKey left, CacheKey right ) => left.Equals( right );

    /// <inheritdoc />
    public static bool operator !=( CacheKey left, CacheKey right ) => !left.Equals( right );
}