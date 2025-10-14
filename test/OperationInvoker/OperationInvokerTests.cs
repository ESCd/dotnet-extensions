using System.Numerics;
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

    [Fact( DisplayName = "Invoke: disposes owned async handlers" )]
    public async Task Invoke_Disposes_OwnedAsyncHandlers( )
    {
        var callback = new HandlerThatDisposesAsync.DisposalCallback();

        using( var services = new ServiceCollection()
            .AddSingleton( callback )
            .AddOperationHandler<HandlerThatDisposesAsync>()
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

    [Fact( DisplayName = "Invoke (unbound generic): invokes operation" )]
    public async Task Invoke_UnboundGeneric_InvokesOperation( )
    {
        using var services = new ServiceCollection()
            .AddOperationHandler( typeof( GenericHandler<> ) )
            .BuildServiceProvider();

        var operations = services.GetRequiredService<IOperationInvoker>();

        Assert.Equal(
            1,
            await operations.Invoke( new GenericOperationIncrement<int>( 0 ) ) );

        Assert.Equal(
            0,
            await operations.Invoke( new GenericOperationDecrement<int>( 1 ) ) );
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
    private sealed record GenericOperationDecrement<T>( T Value ) : IOperation<T> where T : INumber<T>;
    private sealed record GenericOperationIncrement<T>( T Value ) : IOperation<T> where T : INumber<T>;

    private sealed class GenericHandler<T> : IOperationHandler<GenericOperationDecrement<T>, T>, IOperationHandler<GenericOperationIncrement<T>, T>
        where T : INumber<T>
    {
        public ValueTask<T> Invoke( GenericOperationDecrement<T> operation, CancellationToken cancellation ) => new( operation.Value - T.One );
        public ValueTask<T> Invoke( GenericOperationIncrement<T> operation, CancellationToken cancellation ) => new( operation.Value + T.One );
    }

    private sealed class Handler : IOperationHandler<TestOperation>
    {
        public bool WasInvoked { get; private set; }

        public ValueTask Invoke( TestOperation operation, CancellationToken cancellation )
        {
            WasInvoked = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class HandlerThatDisposes( HandlerThatDisposes.DisposalCallback callback ) : IDisposable, IOperationHandler<TestOperation>
    {
        public void Dispose( ) => callback.Invoke();

        public ValueTask Invoke( TestOperation operation, CancellationToken cancellation ) => ValueTask.CompletedTask;

        public sealed record class DisposalCallback
        {
            private bool invoked;
            public void Invoke( ) => invoked = true;

            public static implicit operator bool( DisposalCallback callback ) => callback.invoked;
        }
    }

    private sealed class HandlerThatDisposesAsync( HandlerThatDisposesAsync.DisposalCallback callback ) : IAsyncDisposable, IOperationHandler<TestOperation>
    {
        public ValueTask DisposeAsync( ) => callback.Invoke();

        public ValueTask Invoke( TestOperation operation, CancellationToken cancellation ) => ValueTask.CompletedTask;

        public sealed record class DisposalCallback
        {
            private bool invoked;
            public ValueTask Invoke( )
            {
                invoked = true;
                return default;
            }

            public static implicit operator bool( DisposalCallback callback ) => callback.invoked;
        }
    }

    private sealed class HandlerThatReturns : IOperationHandler<TestOperationWithResult, string>
    {
        public bool WasInvoked { get; private set; }

        public ValueTask<string> Invoke( TestOperationWithResult operation, CancellationToken cancellation )
        {
            WasInvoked = true;
            return ValueTask.FromResult( "Hello, World!" );
        }
    }

    private sealed class HandlerThatThrows : IOperationHandler<TestOperation>, IOperationHandler<TestOperationWithResult, string>
    {
        public ValueTask Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
        public ValueTask<string> Invoke( TestOperationWithResult operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }
}