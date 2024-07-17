namespace ESCd.Extensions.Http.Tests;

public sealed class QueryStringBuilerExtensionTests
{
    [Fact( DisplayName = "Append (bool): appends bool string" )]
    public void Append_Boolean_AppendsBoolString( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", true );

        Assert.Equal( "?test=true", builder.ToString() );
    }

    [Fact( DisplayName = "Append (bool?): does not append null" )]
    public void Append_Boolean_DoesNot_AppendNull( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", default( bool? ) );

        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "Append (DateTime): appends ISO date string" )]
    public void Append_DateTime_AppendsISODateString( )
    {
        var now = DateTime.Now;
        var builder = new QueryStringBuilder()
            .Append( "test", now );

        Assert.Equal( $"?test={Uri.EscapeDataString( now.ToString( "o" ) )}", builder.ToString() );
    }

    [Fact( DisplayName = "Append (DateTime?): does not append null" )]
    public void Append_DateTime_DoesNot_AppendNull( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", default( DateTime? ) );

        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "Append (DateTimeOffset): appends ISO date string" )]
    public void Append_DateTimeOffset_AppendsISODateString( )
    {
        var now = DateTimeOffset.Now;
        var builder = new QueryStringBuilder()
            .Append( "test", now );

        Assert.Equal( $"?test={Uri.EscapeDataString( now.ToString( "o" ) )}", builder.ToString() );
    }

    [Fact( DisplayName = "Append (DateTimeOffset?): does not append null" )]
    public void Append_DateTimeOffset_DoesNot_AppendNull( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", default( DateTimeOffset? ) );

        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "Append (int): appends int string" )]
    public void Append_Int_AppendsIntString( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", int.MaxValue );

        Assert.Equal( $"?test={int.MaxValue}", builder.ToString() );
    }

    [Fact( DisplayName = "Append (int?): does not append null" )]
    public void Append_Int_DoesNot_AppendNull( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", default( int? ) );

        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "Append (long): appends long string" )]
    public void Append_Long_AppendsIntString( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", long.MaxValue );

        Assert.Equal( $"?test={long.MaxValue}", builder.ToString() );
    }

    [Fact( DisplayName = "Append (long?): does not append null" )]
    public void Append_Long_DoesNot_AppendNull( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", default( long? ) );

        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "Append (uint): appends long string" )]
    public void Append_UInt_AppendsIntString( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", uint.MaxValue );

        Assert.Equal( $"?test={uint.MaxValue}", builder.ToString() );
    }

    [Fact( DisplayName = "Append (uint?): does not append null" )]
    public void Append_UInt_DoesNot_AppendNull( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", default( uint? ) );

        Assert.Empty( builder.ToString() );
    }

    [Fact( DisplayName = "Append (ulong): appends ulong string" )]
    public void Append_ULong_AppendsIntString( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", ulong.MaxValue );

        Assert.Equal( $"?test={ulong.MaxValue}", builder.ToString() );
    }

    [Fact( DisplayName = "Append (ulong?): does not append null" )]
    public void Append_ULong_DoesNot_AppendNull( )
    {
        var builder = new QueryStringBuilder()
            .Append( "test", default( ulong? ) );

        Assert.Empty( builder.ToString() );
    }
}