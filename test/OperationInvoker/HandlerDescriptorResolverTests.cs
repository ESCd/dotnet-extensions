using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Tests;

public sealed class HandlerDescriptorResolverTests
{
    [Fact( DisplayName = "Resolve: returns last registered handler" )]
    public void Resolve_Returns_LastRegisteredHandler( )
    {
        using var services = new ServiceCollection()
            .AddOperationHandler<TestHandler>()
            .AddOperationHandler<OtherTestHandler>()
            .BuildServiceProvider();

        var resolver = services.GetRequiredService<HandlerDescriptorResolver>();
        var descriptor = resolver.Resolve( typeof( TestOperation ) );

        Assert.Equal( typeof( OtherTestHandler ), descriptor?.HandlerType );
    }

    private sealed record TestOperation : IOperation;
    private sealed class TestHandler : IOperationHandler<TestOperation>
    {
        public Task Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }

    private sealed class OtherTestHandler : IOperationHandler<TestOperation>
    {
        public Task Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }
}