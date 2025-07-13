using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Benchmarks;

public partial class OperationInvokerBenchmarks : IDisposable
{
    private readonly bool force = Random.Shared.NextDouble() < .5;

    private readonly ServiceProvider serviceProvider = new ServiceCollection()
        .AddOperationHandler<Handler>()
        .BuildServiceProvider();

    public void Dispose( )
    {
        serviceProvider.Dispose();
        GC.SuppressFinalize( this );
    }

    [Benchmark]
    public async Task Invoke( ) => await serviceProvider.InvokeOperation( new Operation( force ) );
}

sealed file record Operation( bool ForceAsync = false ) : IOperation;

sealed file class Handler : IOperationHandler<Operation>
{
    public async Task Invoke( Operation operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );
        if( operation.ForceAsync )
        {
            await Task.Delay( 1, cancellation );
        }
    }
}