using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker.Benchmarks;

public partial class OperationInvokerBenchmarks : IDisposable
{
    private readonly ServiceProvider serviceProvider = new ServiceCollection()
        .AddOperationHandler<Handler>()
        .BuildServiceProvider();

    private readonly bool yield = Random.Shared.NextDouble() < .5;

    public void Dispose( )
    {
        serviceProvider.Dispose();
        GC.SuppressFinalize( this );
    }

    [Benchmark]
    public async Task Invoke( ) => await serviceProvider.InvokeOperation( new Operation( yield ) );
}

sealed file record Operation( bool Yield ) : IOperation;

sealed file class Handler : IOperationHandler<Operation>
{
    public async ValueTask Invoke( Operation operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        if( operation.Yield )
        {
            await Task.Yield();
        }
    }
}