using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Tests;

public sealed class OperationInvokerTests
{
    [Fact( DisplayName = "Invoke: invokes operation" )]
    public async Task Invoke_InvokesOperation( )
    {
        var handler = new Handler();

        var services = new ServiceCollection()
            .AddOperationHandler( handler )
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await invoker.Invoke( new TestOperation() );

        Assert.True( handler.WasInvoked );
    }

    [Fact( DisplayName = "Invoke: returns result of invoking handler" )]
    public async Task Invoke_Returns_ResultOfInvokingHandler( )
    {
        var services = new ServiceCollection()
            .AddOperationHandler<HandlerThatReturns>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();

        Assert.Equal(
            "Hello, World!",
            await invoker.Invoke( new TestOperationWithResult() ) );
    }

    [Fact( DisplayName = "Invoke: throws inner exception of target invocation" )]
    public async Task Invoke_Throws_InvocationInnerException( )
    {
        var services = new ServiceCollection()
            .AddOperationHandler<HandlerThatThrows>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await Assert.ThrowsAsync<NotImplementedException>(
            ( ) => invoker.Invoke( new TestOperation() ) );
    }

    [Fact( DisplayName = "Invoke: throws when operation not registered" )]
    public async Task Invoke_Throws_When_OperationNotRegistered( )
    {
        var services = new ServiceCollection()
            .AddOperationInvoker()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await Assert.ThrowsAsync<ArgumentException>(
            ( ) => invoker.Invoke( new TestOperationWithResult() ) );
    }

    private sealed record TestOperationWithResult : IOperation<string>;
    private sealed record TestOperation : IOperation;

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
        public Task<string> Invoke( TestOperationWithResult operation, CancellationToken cancellation ) => Task.FromResult( "Hello, World!" );
    }

    private sealed class HandlerThatThrows : IOperationHandler<TestOperation>
    {
        public Task Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }
}