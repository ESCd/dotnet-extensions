namespace ESCd.Extensions.Http.Tests;

public sealed class QueryStringBuilerTests
{
    [Fact( DisplayName = "Append: does not append empty value" )]
    public void Append_DoesNot_AppendEmptyValue( )
    {
        var builder = new QueryStringBuilder().Append( "test", "" );
        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "Append: throws when name is null or whitespace" )]
    public void Append_Throws_WhenNameIsNullOrWhiteSpace( )
    {
        var builder = new QueryStringBuilder();

        Assert.Throws<ArgumentNullException>( ( ) => builder.Append( default!, "" ) );
        Assert.Throws<ArgumentException>( ( ) => builder.Append( "", "" ) );
    }

    [Fact( DisplayName = "Builder: can be implicitly cast to string" )]
    public void Builder_Can_BeImplicityCastToString( )
    {
        string value = new QueryStringBuilder();
        Assert.Empty( value );
    }

    [Fact( DisplayName = "Clear: resets builder to empty" )]
    public void Clear_ResetsBuilderToEmpty( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", "Hello, World!" )
            .Clear();

        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "ToString: contains leading question mark" )]
    public void ToString_Contains_LeadingQuestionMark( )
    {
        var builder = new QueryStringBuilder().Append( "test", "test" );
        Assert.StartsWith( "?", builder.ToString() );
    }

    [Fact( DisplayName = "ToString: does not contain trailing ampersand" )]
    public void ToString_DoesNot_Contain_TrailingAmpersand( )
    {
        var builder = new QueryStringBuilder().Append( "test", "test" );
        Assert.False( builder.ToString().EndsWith( '&' ) );
    }

    [Fact( DisplayName = "ToString: returns empty string when builder is empty" )]
    public void ToString_Returns_Empty_WhenBuilderIsEmpty( )
    {
        var builder = new QueryStringBuilder();
        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "ToString: returns escaped query string" )]
    public void ToString_Returns_EscapedQueryString( )
    {
        var value = "Hello, World!";
        var builder = new QueryStringBuilder()
            .Append( "test", [ value, "test" ] );

        Assert.Equal( $"?test={Uri.EscapeDataString( value )}&test=test", builder.ToString() );
    }
}