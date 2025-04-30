using System.Globalization;

namespace ESCd.Extensions.Http;

public static partial class QueryStringBuilderExtensions
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
}