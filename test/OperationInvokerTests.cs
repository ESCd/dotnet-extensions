using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Tests;

public sealed class OperationInvokerTests
{
    [Fact( DisplayName = "Invoke: returns result of invoking handler" )]
    public async Task Invoke_Returns_ResultOfInvokingHandler( )
    {
        var services = new ServiceCollection()
            .AddOperationHandler<HandlerThatReturns>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();

        Assert.Equal(
            "Hello, World!",
            await invoker.Invoke( new TestOperation() ) );
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
            ( ) => invoker.Invoke( new TestOperation() ) );
    }

    private sealed record TestOperation : IOperation<string>;
    private sealed class HandlerThatThrows : IOperationHandler<TestOperation, string>
    {
        public Task<string> Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }

    private sealed class HandlerThatReturns : IOperationHandler<TestOperation, string>
    {
        public Task<string> Invoke( TestOperation operation, CancellationToken cancellation ) => Task.FromResult( "Hello, World!" );
    }
}