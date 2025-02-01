using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Tests;

public sealed class ServiceProviderExtensionsTests
{
    [Fact( DisplayName = "InvokeOperation: invokes operation" )]
    public async Task InvokeOperation_InvokesOperation( )
    {
        var handler = new Handler();
        using( var services = new ServiceCollection()
            .AddOperationHandler( handler )
            .BuildServiceProvider() )
        {
            await services.InvokeOperation( new TestOperation() );
        }

        Assert.True( handler.WasInvoked );
    }

    [Fact( DisplayName = "InvokeOperation (typed result): invokes operation" )]
    public async Task InvokeOperation_TypedResult_InvokesOperation( )
    {
        var handler = new HandlerThatReturns();
        using( var services = new ServiceCollection()
            .AddOperationHandler( handler )
            .BuildServiceProvider() )
        {
            var result = await services.InvokeOperation( new TestOperationWithResult() );
            Assert.Equal( "Hello, World!", result );
        }

        Assert.True( handler.WasInvoked );
    }

    private sealed class Handler : IOperationHandler<TestOperation>
    {
        public bool WasInvoked { get; private set; }

        public Task Invoke( TestOperation operation, CancellationToken cancellation )
        {
            WasInvoked = true;
            return Task.CompletedTask;
        }
    }

    private sealed class HandlerThatReturns : IOperationHandler<TestOperationWithResult, string>
    {
        public bool WasInvoked { get; private set; }

        public Task<string> Invoke( TestOperationWithResult operation, CancellationToken cancellation )
        {
            WasInvoked = true;
            return Task.FromResult( "Hello, World!" );
        }
    }

    private sealed record TestOperationWithResult : IOperation<string>;
    private sealed record TestOperation : IOperation;
}