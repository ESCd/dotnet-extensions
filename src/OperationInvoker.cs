using System.Reflection;
using System.Runtime.CompilerServices;
using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker;

internal sealed class OperationInvoker( HandlerDescriptorFinder descriptorFinder, IServiceProvider serviceProvider ) : IOperationInvoker
{
    public async Task Invoke( IOperation operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        var (descriptor, handler) = ResolveHandler( operation.GetType() );

        try
        {
            await Unsafe.As<Task>(
                descriptor.InvokeMethod.Invoke( handler, [ operation, cancellation ] )! ).ConfigureAwait( false );
        }
        catch( TargetInvocationException exception )
        {
            // NOTE: rethrow the exception of the target invocation
            throw exception.InnerException!;
        }
    }

    public async Task<TResult> Invoke<TResult>( IOperation<TResult> operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        var (descriptor, handler) = ResolveHandler( operation.GetType() );

        try
        {
            return await Unsafe.As<Task<TResult>>(
                descriptor.InvokeMethod.Invoke( handler, [ operation, cancellation ] )! ).ConfigureAwait( false );
        }
        catch( TargetInvocationException exception )
        {
            // NOTE: rethrow the exception of the target invocation
            throw exception.InnerException!;
        }
    }

    private (OperationHandlerDescriptor Descriptor, object Handler) ResolveHandler( Type type )
    {
        var descriptor = descriptorFinder.Find( type ) ?? throw new ArgumentException( $"An IOperationHandler for {type} has not been registered to the service provider.", nameof( type ) );
        return (
            descriptor,
            descriptor.Instance ?? ActivatorUtilities.CreateInstance( serviceProvider, descriptor.HandlerType ));
    }
};

internal sealed class HandlerDescriptorFinder( IEnumerable<OperationHandlerDescriptor> descriptors )
{
    private readonly Dictionary<Type, OperationHandlerDescriptor> descriptorsByOperationType = descriptors.ToDictionary( handler => handler.OperationType );

    public OperationHandlerDescriptor? Find( Type operationType )
    {
        if( descriptorsByOperationType.TryGetValue( operationType, out var descriptor ) )
        {
            return descriptor;
        }

        return default;
    }
}