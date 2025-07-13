using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Benchmarks;

public partial class OperationInvokerBenchmarks : IDisposable
{
    private readonly ServiceProvider serviceProvider;

    public OperationInvokerBenchmarks( )
    {
        serviceProvider = new ServiceCollection()
            .AddOperationHandler<Handler>()
            .BuildServiceProvider();
    }

    public void Dispose( )
    {
        serviceProvider.Dispose();
        GC.SuppressFinalize( this );
    }

    [Benchmark]
    public async Task Invoke( ) => await serviceProvider.InvokeOperation( new Operation() );
}

sealed file record Operation : IOperation;

sealed file class Handler : IOperationHandler<Operation>
{
    public Task Invoke( Operation operation, CancellationToken cancellation ) => Task.CompletedTask;
}