namespace ESCd.Extensions.Http.Tests;

public sealed class PooledQueryStringBuilderTests
{
    [Fact( DisplayName = "Policy: does not return builders exceeding capacity" )]
    public void Policy_DoesNot_Return_BuildersExceedingCapacity( )
    {
        var policy = new PooledQueryStringBuilderPolicy
        {
            MaximumRetainedCapacity = 10
        };

        var builder = policy.Create()
            .Append( "test", "123456789" );

        Assert.False( policy.Return( builder ) );
    }

    [Fact( DisplayName = "Policy: uses initial capacity" )]
    public void Policy_Uses_InitialCapacity( )
    {
        var policy = new PooledQueryStringBuilderPolicy
        {
            InitialCapacity = 10
        };

        var builder = policy.Create();
        Assert.Equal( 10, builder.Capacity );
    }
}