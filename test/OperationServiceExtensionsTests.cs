using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Tests;

public sealed class OperationServiceExtensionsTests
{
    [Fact( DisplayName = "AddOperationHandler: adds descriptors for handler" )]
    public async Task AddOperationHandler_Adds_DescriptorsForHandler( )
    {
        var services = new ServiceCollection()
            .AddOperationHandler<TestHandler>()
            .BuildServiceProvider();

        var descriptors = services.GetServices<OperationHandlerDescriptor>().ToArray();

        Assert.Equal( 2, descriptors.Length );
        Assert.Equal( typeof( TestOperation ), descriptors[ 0 ].OperationType );
        Assert.Equal( typeof( OtherTestOperation ), descriptors[ 1 ].OperationType );
    }

    [Fact( DisplayName = "AddOperationHandler: adds invoker" )]
    public async Task AddOperationHandler_Adds_OperationInvoker( )
    {
        var services = new ServiceCollection()
            .AddOperationHandler<TestHandler>()
            .BuildServiceProvider();

        var invoker = services.GetRequiredService<IOperationInvoker>();
        Assert.NotNull( invoker );
    }

    [Fact( DisplayName = "AddOperationHandler: throws when type does not implement handler" )]
    public async Task AddOperationHandler_Throws_When_TypeDoesNotImplementHandler( )
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(
            ( ) => services.AddOperationHandler<TestOperation>() );
    }

    private sealed record TestOperation : IOperation;
    private sealed record OtherTestOperation : IOperation;

    private sealed class TestHandler : IOperationHandler<TestOperation>, IOperationHandler<OtherTestOperation>
    {
        public Task Invoke( TestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
        public Task Invoke( OtherTestOperation operation, CancellationToken cancellation ) => throw new NotImplementedException();
    }
}