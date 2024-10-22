using System.Reflection;
using System.Runtime.CompilerServices;
using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker;

internal sealed class OperationInvoker( HandlerDescriptorResolver descriptorResolver, IServiceProvider serviceProvider ) : IOperationInvoker
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
            throw exception.InnerException ?? exception;
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
            throw exception.InnerException ?? exception;
        }
    }

    private (OperationHandlerDescriptor Descriptor, object Handler) ResolveHandler( Type type )
    {
        var descriptor = descriptorResolver.Resolve( type ) ?? throw new ArgumentException( $"An IOperationHandler for {type} has not been registered to the service provider.", nameof( type ) );
        return (
            descriptor,
            descriptor.Instance ?? ActivatorUtilities.CreateInstance( serviceProvider, descriptor.HandlerType ));
    }
};