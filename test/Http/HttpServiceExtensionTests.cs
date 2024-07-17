using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace ESCd.Extensions.Http.Tests;

public sealed class HttpServiceExtensionTests
{
    [Fact( DisplayName = "AddQueryStringBuilderObjectPool: adds object pool" )]
    public void AddQueryStringBuilderObjectPool_Adds_ObjectPools( )
    {
        using var services = new ServiceCollection()
            .AddQueryStringBuilderObjectPool()
            .BuildServiceProvider();

        Assert.NotNull(
            services.GetRequiredService<ObjectPool<QueryStringBuilder>>() );
    }
}