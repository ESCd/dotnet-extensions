using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ESCd.Extensions.OperationInvoker.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ESCd.Extensions.OperationInvoker;

/// <summary> Extensions for registering Operation Handlers. </summary>
public static class OperationServiceExtensions
{
    /// <summary> Add <see cref="OperationHandlerDescriptor"/>s for the handlers implemented by the given <typeparamref name="THandler"/>. </summary>
    /// <typeparam name="THandler"> A type that implements <see cref="IOperationHandler{T}"/>. </typeparam>
    /// <param name="services"> The collection of services to add the handlers to. </param>
    /// <param name="instance"> An (optional) singleton instance to register. </param>
    /// <exception cref="ArgumentException"> The given <typeparamref name="THandler"/> type does not implement any operations. </exception>
    [DynamicDependency( DynamicallyAccessedMemberTypes.PublicMethods, typeof( IOperationHandler<> ) )]
    [DynamicDependency( DynamicallyAccessedMemberTypes.PublicMethods, typeof( IOperationHandler<,> ) )]
    public static IServiceCollection AddOperationHandler<[DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] THandler>( this IServiceCollection services, THandler? instance = null )
        where THandler : class
    {
        ArgumentNullException.ThrowIfNull( services );

        var descriptors = CreateHandlerDescriptors( instance );
        if( descriptors.Length is 0 )
        {
            throw new ArgumentException( $"Given type does not implement {typeof( IOperationHandler<> ).Name}.", nameof( THandler ) );
        }

        return AddOperationInvoker( services ).Add( descriptors );
    }

    /// <summary> Adds the default <see cref="IOperationInvoker"/>. </summary>
    public static IServiceCollection AddOperationInvoker( this IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        services.TryAddSingleton<HandlerDescriptorFinder>();
        services.TryAddTransient<IOperationInvoker, OperationInvoker>();
        return services;
    }

    private static ServiceDescriptor[] CreateHandlerDescriptors<[DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods )] THandler>( THandler? instance = null )
        where THandler : class
        => [ .. typeof( THandler ).GetInterfaces()
            .Where( static type =>
            {
                if( !type.IsGenericType )
                {
                    return false;
                }

                var definition = type.GetGenericTypeDefinition();
                return definition == typeof( IOperationHandler<> ) || definition == typeof( IOperationHandler<,> );
            } )
            .Select( ( [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicMethods )] type ) => new OperationHandlerDescriptor
            {
                HandlerType = typeof( THandler ),
                Instance = instance,
                InvokeMethod = type.GetMethod( "Invoke", BindingFlags.Instance | BindingFlags.Public ) ?? throw new MissingMethodException( $"Type '{typeof( THandler ).FullName}' is missing method 'Invoke'. This may be the result of code trimming." ),
                OperationType = type.GenericTypeArguments[ 0 ],
            } )
            .Select( ServiceDescriptor.Singleton ) ];
}

/// <summary> Represents the metadata of an <see cref="IOperationHandler{T}"/> implementation. </summary>
public sealed class OperationHandlerDescriptor
{
    /// <summary> The type of the handler implementation. </summary>
    [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods )]
    public Type HandlerType { get; internal init; }

    /// <summary> A singleton instance of the handler. </summary>
    public object? Instance { get; internal init; }

    /// <summary> A reference to the <see cref="IOperationHandler{T}.Invoke(T, CancellationToken)"/> method.. </summary>
    public MethodInfo InvokeMethod { get; internal init; }

    /// <summary> The type of <see cref="IOperation"/> handled by the implementation. </summary>
    public Type OperationType { get; internal init; }
}