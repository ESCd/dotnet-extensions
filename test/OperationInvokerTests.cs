using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Tests;

public sealed class OperationInvokerTests
{
    [Fact( DisplayName = "Invoke: invokes operation" )]
    public async Task Invoke_InvokesOperation( )
    {
        var handler = new Handler();
        using var services = new ServiceCollection()
            .AddOperationHandler( handler )
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await invoker.Invoke( new TestOperation() );

        Assert.True( handler.WasInvoked );
    }

    [Fact( DisplayName = "Invoke (typed result): invokes operation" )]
    public async Task Invoke_TypedResult_InvokesOperation( )
    {
        var handler = new HandlerThatReturns();
        using var services = new ServiceCollection()
            .AddOperationHandler( handler )
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();

        var result = await invoker.Invoke( new TestOperationWithResult() );
        Assert.True( handler.WasInvoked );
        Assert.Equal( "Hello, World!", result );
    }

    [Fact( DisplayName = "Invoke: throws inner exception of target invocation" )]
    public async Task Invoke_Throws_InvocationInnerException( )
    {
        using var services = new ServiceCollection()
            .AddOperationHandler<HandlerThatThrows>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await Assert.ThrowsAsync<NotImplementedException>(
            ( ) => invoker.Invoke( new TestOperation() ) );
    }

    [Fact( DisplayName = "Invoke (typed result): throws inner exception of target invocation" )]
    public async Task Invoke_TypedOperation_Throws_InvocationInnerException( )
    {
        using var services = new ServiceCollection()
            .AddOperationHandler<HandlerThatThrows>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await Assert.ThrowsAsync<NotImplementedException>(
            ( ) => invoker.Invoke( new TestOperationWithResult() ) );
    }

    [Fact( DisplayName = "Invoke: throws when operation not registered" )]
    public async Task Invoke_Throws_When_OperationNotRegistered( )
    {
        using var services = new ServiceCollection()
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
        public bool WasInvoked { get; private set; }

        public Task<string> Invoke( TestOperationWithResult operation, CancellationToken cancellation )
        {
            WasInvoked = true;
            return Task.FromResult( "Hello, World!" );
        }
    }

    private sealed class HandlerThatThrows : IOperationHandler<TestOperation>, IOperationHandler<TestOperationWithResult, string>
    {
        public Task Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
        public Task<string> Invoke( TestOperationWithResult operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }
}