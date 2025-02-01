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

        await using var invoker = new OperationHandlerInvoker(
            ResolveHandlerDescriptor( operation.GetType() ),
            serviceProvider );

        await invoker.Invoke( operation, cancellation );
    }

    public async Task<TResult> Invoke<TResult>( IOperation<TResult> operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        await using var invoker = new OperationHandlerInvoker<TResult>(
            ResolveHandlerDescriptor( operation.GetType() ),
            serviceProvider );

        return await invoker.Invoke( operation, cancellation );
    }

    private OperationHandlerDescriptor ResolveHandlerDescriptor( Type type )
    {
        ArgumentNullException.ThrowIfNull( type );
        return descriptorResolver.Resolve( type ) ?? throw new ArgumentException( $"An IOperationHandler for {type} has not been registered to the service provider.", nameof( type ) );
    }
};

sealed file class OperationHandlerInvoker( OperationHandlerDescriptor descriptor, IServiceProvider serviceProvider ) : IAsyncDisposable
{
    private readonly OperationHandlerInstance instance = OperationHandlerInstance.Create( serviceProvider, descriptor );

    public ValueTask DisposeAsync( ) => instance.DisposeAsync();

    public Task Invoke( IOperation operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        try
        {
            return Unsafe.As<Task>( descriptor.InvokeMethod.Invoke( instance.Value, [ operation, cancellation ] ) )!;
        }
        catch( TargetInvocationException exception )
        {
            // NOTE: rethrow the exception of the target invocation
            throw exception.InnerException ?? exception;
        }
    }
}

sealed file class OperationHandlerInvoker<TResult>( OperationHandlerDescriptor descriptor, IServiceProvider serviceProvider ) : IAsyncDisposable
{
    private readonly OperationHandlerInstance instance = OperationHandlerInstance.Create( serviceProvider, descriptor );

    public ValueTask DisposeAsync( ) => instance.DisposeAsync();

    public Task<TResult> Invoke( IOperation<TResult> operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        try
        {
            return Unsafe.As<Task<TResult>>( descriptor.InvokeMethod.Invoke( instance.Value, [ operation, cancellation ] ) )!;
        }
        catch( TargetInvocationException exception )
        {
            // NOTE: rethrow the exception of the target invocation
            throw exception.InnerException ?? exception;
        }
    }
}

sealed file class OperationHandlerInstance : IAsyncDisposable
{
    private readonly bool owned;
    public object Value { get; }

    private OperationHandlerInstance( object value, bool owned )
    {
        this.owned = owned;
        Value = value;
    }

    public static OperationHandlerInstance Create( IServiceProvider serviceProvider, OperationHandlerDescriptor descriptor )
    {
        ArgumentNullException.ThrowIfNull( serviceProvider );
        ArgumentNullException.ThrowIfNull( descriptor );

        if( descriptor.Instance is not null )
        {
            return new( descriptor.Instance, false );
        }

        return new(
            ActivatorUtilities.CreateInstance( serviceProvider, descriptor.HandlerType ),
            true );
    }

    public ValueTask DisposeAsync( )
    {
        if( owned )
        {
            if( Value is IAsyncDisposable async )
            {
                return async.DisposeAsync();
            }

            if( Value is IDisposable disposable )
            {
                disposable.Dispose();
            }
        }

        return default;
    }
}