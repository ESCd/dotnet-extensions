using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Tests;

public sealed class OperationInvokerTests
{
    [Fact( DisplayName = "Invoke: disposes owned handlers" )]
    public async Task Invoke_Disposes_OwnedHandlers( )
    {
        var callback = new HandlerThatDisposes.DisposalCallback();

        using( var services = new ServiceCollection()
            .AddSingleton( callback )
            .AddOperationHandler<HandlerThatDisposes>()
            .BuildServiceProvider() )
        {
            await services.GetRequiredService<IOperationInvoker>().Invoke( new TestOperation() );
        }

        Assert.True( callback );
    }

    [Fact( DisplayName = "Invoke: invokes operation" )]
    public async Task Invoke_InvokesOperation( )
    {
        var handler = new Handler();

        using( var services = new ServiceCollection()
            .AddOperationHandler( handler )
            .BuildServiceProvider() )
        {
            await services.GetRequiredService<IOperationInvoker>().Invoke( new TestOperation() );
        }

        Assert.True( handler.WasInvoked );
    }

    [Fact( DisplayName = "Invoke (typed result): invokes operation" )]
    public async Task Invoke_TypedResult_InvokesOperation( )
    {
        var handler = new HandlerThatReturns();

        using( var services = new ServiceCollection()
            .AddOperationHandler( handler )
            .BuildServiceProvider() )
        {
            var result = await services.GetRequiredService<IOperationInvoker>()
                .Invoke( new TestOperationWithResult() );

            Assert.Equal( "Hello, World!", result );
        }

        Assert.True( handler.WasInvoked );
    }

    [Fact( DisplayName = "Invoke: throws inner exception of target invocation" )]
    public async Task Invoke_Throws_InvocationInnerException( )
    {
        using var services = new ServiceCollection()
            .AddOperationHandler<HandlerThatThrows>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await Assert.ThrowsAsync<NotImplementedException>(
            async ( ) => await invoker.Invoke( new TestOperation() ) );
    }

    [Fact( DisplayName = "Invoke (typed result): throws inner exception of target invocation" )]
    public async Task Invoke_TypedOperation_Throws_InvocationInnerException( )
    {
        using var services = new ServiceCollection()
            .AddOperationHandler<HandlerThatThrows>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await Assert.ThrowsAsync<NotImplementedException>(
            async ( ) => await invoker.Invoke( new TestOperationWithResult() ) );
    }

    [Fact( DisplayName = "Invoke: throws when operation not registered" )]
    public async Task Invoke_Throws_When_OperationNotRegistered( )
    {
        using var services = new ServiceCollection()
            .AddOperationInvoker()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        await Assert.ThrowsAsync<ArgumentException>(
            async ( ) => await invoker.Invoke( new TestOperationWithResult() ) );
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

    private sealed class HandlerThatDisposes( HandlerThatDisposes.DisposalCallback callback ) : IDisposable, IOperationHandler<TestOperation>
    {
        public void Dispose( ) => callback.Invoke();

        public Task Invoke( TestOperation operation, CancellationToken cancellation ) => Task.CompletedTask;

        public sealed record class DisposalCallback
        {
            private bool invoked;
            public void Invoke( ) => invoked = true;

            public static implicit operator bool( DisposalCallback callback ) => callback.invoked;
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