using System.Reflection;
using System.Runtime.CompilerServices;
using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ESCd.Extensions.OperationInvoker;

internal sealed class OperationInvoker( HandlerDescriptorResolver resolver, IServiceProvider services ) : IOperationInvoker
{
    public async ValueTask Invoke( IOperation operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        var descriptor = ResolveHandlerDescriptor( operation.GetType() );

        await using var invoker = new OperationHandlerInvoker( descriptor, services );
        await invoker.Invoke( operation, cancellation ).ConfigureAwait( false );
    }

    public async ValueTask<TResult> Invoke<TResult>( IOperation<TResult> operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        var descriptor = ResolveHandlerDescriptor( operation.GetType() );

        await using var invoker = new OperationHandlerInvoker<TResult>( descriptor, services );
        return await invoker.Invoke( operation, cancellation ).ConfigureAwait( false );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private OperationHandlerDescriptor ResolveHandlerDescriptor( Type type ) => resolver.Resolve( type ) ?? throw new ArgumentException( $"An IOperationHandler for {type} has not been registered to the service provider.", nameof( type ) );
};

sealed file class OperationHandlerInvoker( OperationHandlerDescriptor descriptor, IServiceProvider serviceProvider ) : IAsyncDisposable
{
    private readonly OperationHandlerInstance instance = OperationHandlerInstance.Create( serviceProvider, descriptor );

    public ValueTask DisposeAsync( ) => instance.DisposeAsync();

    public async ValueTask Invoke( IOperation operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        var invoke = Unsafe.As<Func<object, IOperation, CancellationToken, ValueTask>>( descriptor.Invoke );
        try
        {
            await invoke( instance.Value, operation, cancellation ).ConfigureAwait( false );
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
    private readonly OperationHandlerDescriptor descriptor = descriptor;
    private readonly OperationHandlerInstance instance = OperationHandlerInstance.Create( serviceProvider, descriptor );

    public ValueTask DisposeAsync( ) => instance.DisposeAsync();

    public async ValueTask<TResult> Invoke( IOperation<TResult> operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        var invoke = Unsafe.As<Func<object, IOperation<TResult>, CancellationToken, ValueTask<TResult>>>( descriptor.Invoke );
        try
        {
            return await invoke( instance.Value, operation, cancellation ).ConfigureAwait( false );
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

        return new( ActivatorUtilities.CreateInstance( serviceProvider, descriptor.HandlerType ), true );
    }

    public async ValueTask DisposeAsync( )
    {
        if( owned )
        {
            if( Value is IAsyncDisposable async )
            {
                await async.DisposeAsync().ConfigureAwait( false );
            }
            else if( Value is IDisposable disposable )
            {
                disposable.Dispose();
            }
        }
    }
}