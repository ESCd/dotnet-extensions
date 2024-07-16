using System.Globalization;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace ESCd.Extensions.Http;

/// <summary> Provides a "fluent builder" syntax for constructing query string parameters. </summary>
/// <remarks> This class cannot be inherited. </remarks>
public sealed class QueryStringBuilder
{
    private readonly StringBuilder builder;

    /// <summary> An <see cref="ObjectPool{T}"/> of <see cref="QueryStringBuilder"/>s. </summary>
    public static readonly ObjectPool<QueryStringBuilder> Pool = ObjectPool.Create( new QueryStringBuilderPooledObjectPolicy() );

    /// <summary> Create an empty query string. </summary>
    public QueryStringBuilder( )
        => builder = new( "?" );

    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public QueryStringBuilder Append( string name, string? value )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace( name );
        if( !string.IsNullOrWhiteSpace( value ) )
        {
            builder.Append( CultureInfo.InvariantCulture, $"{name}={Uri.EscapeDataString( value )}&" );
        }

        return this;
    }

    /// <summary> Appends a parameter with the given <paramref name="name"/> for each of the given <paramref name="values"/>. </summary>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="values"> The values of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public QueryStringBuilder Append( string name, IEnumerable<string?> values )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace( name );
        foreach( var value in values )
        {
            if( !string.IsNullOrWhiteSpace( value ) )
            {
                builder.Append( CultureInfo.InvariantCulture, $"{name}={Uri.EscapeDataString( value )}&" );
            }
        }

        return this;
    }

    /// <summary> Remove all parameters from the current builder. </summary>
    public QueryStringBuilder Clear( )
    {
        builder.Clear().Append( '?' );
        return this;
    }

    /// <inheritdoc/>
    public override string ToString( )
    {
        var value = builder.ToString().TrimEnd( '&' );
        if( value.Length is 1 )
        {
            return string.Empty;
        }

        return value;
    }

    /// <inheritdoc/>
    public static implicit operator string( QueryStringBuilder builder ) => builder.ToString();
}

/// <summary> Extensions to <see cref="QueryStringBuilder"/> for appending typed values. </summary>
public static class QueryStringBuilderExtensions
{
    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="builder"> The builder to be mutated </param>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public static QueryStringBuilder Append( this QueryStringBuilder builder, string name, DateTime? value )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( value.HasValue )
        {
            return builder.Append( name, value.Value.ToString( "o", CultureInfo.InvariantCulture ) );
        }

        return builder;
    }

    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="builder"> The builder to be mutated </param>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public static QueryStringBuilder Append( this QueryStringBuilder builder, string name, DateTimeOffset? value )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( value.HasValue )
        {
            return builder.Append( name, value.Value.ToString( "o", CultureInfo.InvariantCulture ) );
        }

        return builder;
    }

    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="builder"> The builder to be mutated </param>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public static QueryStringBuilder Append( this QueryStringBuilder builder, string name, bool? value )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( value.HasValue )
        {
            return builder.Append( name, value.Value.ToString( CultureInfo.InvariantCulture ) );
        }

        return builder;
    }

    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="builder"> The builder to be mutated </param>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public static QueryStringBuilder Append( this QueryStringBuilder builder, string name, long? value )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( value.HasValue )
        {
            return builder.Append( name, value.Value.ToString( CultureInfo.InvariantCulture ) );
        }

        return builder;
    }

    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="builder"> The builder to be mutated </param>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public static QueryStringBuilder Append( this QueryStringBuilder builder, string name, int? value )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( value.HasValue )
        {
            return builder.Append( name, value.Value.ToString( CultureInfo.InvariantCulture ) );
        }

        return builder;
    }

    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="builder"> The builder to be mutated </param>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public static QueryStringBuilder Append( this QueryStringBuilder builder, string name, uint? value )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( value.HasValue )
        {
            return builder.Append( name, value.Value.ToString( CultureInfo.InvariantCulture ) );
        }

        return builder;
    }

    /// <summary> Appends a parameter with the given <paramref name="name"/> and <paramref name="value"/>. </summary>
    /// <param name="builder"> The builder to be mutated </param>
    /// <param name="name"> The name of the parameter. </param>
    /// <param name="value"> The value of the parameter. </param>
    /// <returns> The mutated query string. </returns>
    public static QueryStringBuilder Append( this QueryStringBuilder builder, string name, ulong? value )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( value.HasValue )
        {
            return builder.Append( name, value.Value.ToString( CultureInfo.InvariantCulture ) );
        }

        return builder;
    }
}