using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker;

/// <summary> Extensions for invoking operations from a Service Provider. </summary>
public static class ServiceProviderExtensions
{
    /// <summary> Invoke an operation. </summary>
    /// <param name="serviceProvider"> The service provider to resolve the <see cref="IOperationInvoker"/> from. </param>
    /// <param name="operation"> The operation to invoke. </param>
    /// <param name="cancellation"> A token to trigger cancellation of the operation. </param>
    public static Task<T> InvokeOperation<T>( this IServiceProvider serviceProvider, IOperation<T> operation, CancellationToken cancellation = default )
    {
        ArgumentNullException.ThrowIfNull( serviceProvider );
        ArgumentNullException.ThrowIfNull( operation );

        return serviceProvider.GetRequiredService<IOperationInvoker>()
            .Invoke( operation, cancellation );
    }

    /// <summary> Invoke an operation. </summary>
    /// <param name="serviceProvider"> The service provider to resolve the <see cref="IOperationInvoker"/> from. </param>
    /// <param name="operation"> The operation to invoke. </param>
    /// <param name="cancellation"> A token to trigger cancellation of the operation. </param>
    public static Task InvokeOperation( this IServiceProvider serviceProvider, IOperation operation, CancellationToken cancellation = default )
    {
        ArgumentNullException.ThrowIfNull( serviceProvider );
        ArgumentNullException.ThrowIfNull( operation );

        return serviceProvider.GetRequiredService<IOperationInvoker>()
            .Invoke( operation, cancellation );
    }
}