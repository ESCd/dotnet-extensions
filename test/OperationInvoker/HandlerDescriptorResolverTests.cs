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

        var descriptor = services.GetRequiredService<HandlerDescriptorResolver>()
            .Resolve( typeof( TestOperation ) );

        Assert.Equal( typeof( OtherTestHandler ), descriptor?.HandlerType );
    }

    [Fact( DisplayName = "Resolve: returns unbound generic handler" )]
    public void Resolve_Returns_UnboundGenericHandler( )
    {
        using var services = new ServiceCollection()
            .AddOperationHandler( typeof( GenericHandler<> ) )
            .BuildServiceProvider();

        var descriptor = services.GetRequiredService<HandlerDescriptorResolver>()
            .Resolve( typeof( GenericOperation<int> ) );

        Assert.Equal( typeof( GenericHandler<int> ), descriptor?.HandlerType );
        Assert.Equal( typeof( GenericOperation<int> ), descriptor?.OperationType );
    }

    private sealed record GenericOperation<T> : IOperation;
    private sealed record TestOperation : IOperation;
    private sealed class GenericHandler<T> : IOperationHandler<GenericOperation<T>>
    {
        public ValueTask Invoke( GenericOperation<T> operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }

    private sealed class TestHandler : IOperationHandler<TestOperation>
    {
        public ValueTask Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }

    private sealed class OtherTestHandler : IOperationHandler<TestOperation>
    {
        public ValueTask Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }
}