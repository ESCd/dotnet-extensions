using System.Globalization;
using System.Text;

namespace ESCd.Extensions.Http;

/// <summary> Provides a "fluent builder" syntax for constructing query string parameters. </summary>
/// <remarks> This class cannot be inherited. </remarks>
public sealed class QueryStringBuilder
{
    private readonly StringBuilder builder;

    /// <summary> Get or set the suggested size of the instance. </summary>
    public int Capacity { get => builder.Capacity; set => builder.Capacity = value; }

    internal int Length { get => builder.Length; set => builder.Length = value; }

    /// <summary> Create an empty query string. </summary>
    public QueryStringBuilder( )
    {
        builder = new( "?" );
    }

    /// <summary> Create an empty query string. </summary>
    /// <param name="capacity"> The suggested starting size of the instance. </param>
    public QueryStringBuilder( int capacity )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero( capacity );
        builder = new( "?", Math.Max( 1, capacity ) );
    }

    /// <summary> Create a query string from the given <paramref name="url"/>. </summary>
    public QueryStringBuilder( Uri? url ) : this( url?.Query )
    {
    }

    /// <summary> Create a query string from the given <paramref name="value"/>. </summary>
    public QueryStringBuilder( string? value )
    {
        builder = new(
            !string.IsNullOrWhiteSpace( value ) ? $"?{value?.Trim( '?', '&' )}&" : "?" );
    }

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
    public QueryStringBuilder Append( string name, IEnumerable<string?>? values )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        if( values is not null )
        {
            foreach( var value in values )
            {
                if( !string.IsNullOrWhiteSpace( value ) )
                {
                    builder.Append( CultureInfo.InvariantCulture, $"{name}={Uri.EscapeDataString( value )}&" );
                }
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
public static partial class QueryStringBuilderExtensions
{
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
            return builder.Append( name, value.Value.ToString( CultureInfo.InvariantCulture ).ToLowerInvariant() );
        }

        return builder;
    }
}