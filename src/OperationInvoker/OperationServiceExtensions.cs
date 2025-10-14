using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    /// <exception cref="ArgumentException"> The given <typeparamref name="THandler"/> type does not implement any operations. </exception>
    public static IServiceCollection AddOperationHandler<[DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] THandler>( this IServiceCollection services )
        where THandler : class
    {
        ArgumentNullException.ThrowIfNull( services );
        return AddOperationHandler( services, typeof( THandler ) );
    }

    /// <summary> Add <see cref="OperationHandlerDescriptor"/>s for the handlers implemented by the given <typeparamref name="THandler"/>. </summary>
    /// <typeparam name="THandler"> A type that implements <see cref="IOperationHandler{T}"/>. </typeparam>
    /// <param name="services"> The collection of services to add the handlers to. </param>
    /// <param name="instance"> An (optional) singleton instance to register. </param>
    /// <exception cref="ArgumentException"> The given <typeparamref name="THandler"/> type does not implement any operations. </exception>
    public static IServiceCollection AddOperationHandler<[DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] THandler>( this IServiceCollection services, THandler instance )
        where THandler : class
    {
        ArgumentNullException.ThrowIfNull( services );
        return AddOperationHandler( services, typeof( THandler ), instance );
    }

    /// <summary> Add <see cref="OperationHandlerDescriptor"/>s for the handlers implemented by the given <paramref name="type"/>. </summary>
    /// <param name="type"> A type that implements <see cref="IOperationHandler{T}"/>. </param>
    /// <param name="services"> The collection of services to add the handlers to. </param>
    /// <exception cref="ArgumentException"> The given <paramref name="type"/> does not implement any operations. </exception>
    public static IServiceCollection AddOperationHandler( this IServiceCollection services, [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type type )
    {
        ArgumentNullException.ThrowIfNull( services );
        ArgumentNullException.ThrowIfNull( type );

        return AddOperationHandler( services, type, default );
    }

    /// <summary> Add <see cref="OperationHandlerDescriptor"/>s for the handlers implemented by the given <paramref name="type"/>. </summary>
    /// <param name="type"> A type that implements <see cref="IOperationHandler{T}"/>. </param>
    /// <param name="services"> The collection of services to add the handlers to. </param>
    /// <param name="instance"> An (optional) singleton instance to register. </param>
    /// <exception cref="ArgumentException"> The given <paramref name="type"/> does not implement any operations. </exception>
    [DynamicDependency( DynamicallyAccessedMemberTypes.PublicMethods, typeof( IOperationHandler<> ) )]
    [DynamicDependency( DynamicallyAccessedMemberTypes.PublicMethods, typeof( IOperationHandler<,> ) )]
    public static IServiceCollection AddOperationHandler( this IServiceCollection services, [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type type, object? instance = null )
    {
        ArgumentNullException.ThrowIfNull( services );
        ArgumentNullException.ThrowIfNull( type );

        if( instance is not null && !type.IsInstanceOfType( instance ) )
        {
            throw new ArgumentException( $"The given instance is not of the given type '{type.FullName}'.", nameof( instance ) );
        }

        var descriptors = CreateHandlerDescriptors( type, instance );
        if( descriptors.Length is 0 )
        {
            throw new ArgumentException( $"Given type does not implement {typeof( IOperationHandler<> ).Name}.", nameof( type ) );
        }

        return AddOperationInvoker( services ).Add( FilterDuplicates( services, descriptors ).Select( ServiceDescriptor.Singleton ) );
    }

    /// <summary> Adds the default <see cref="IOperationInvoker"/>. </summary>
    public static IServiceCollection AddOperationInvoker( this IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        services.TryAddSingleton<HandlerDescriptorResolver>();
        services.TryAddTransient<IOperationInvoker, OperationInvoker>();
        return services;
    }

    internal static OperationHandlerDescriptor CreateHandlerDescriptor(
        [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type handler,
        [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type operation,
        object? instance = null )
    {
        ArgumentNullException.ThrowIfNull( handler );
        ArgumentNullException.ThrowIfNull( operation );

        var descriptor = OperationHandlerDescriptor.Create(
            FindDefinition( handler, operation ),
            handler,
            instance );

        return descriptor with
        {
            OperationType = operation,
        };

        [return: DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicMethods )]
        static Type FindDefinition(
            [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type handler,
            [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type operation )
        {
            ArgumentNullException.ThrowIfNull( handler );
            ArgumentNullException.ThrowIfNull( operation );

#pragma warning disable IL2073
            return handler.FindInterfaces( HandlerInterfaceFilter, operation ).Single().GetTypeInfo();
#pragma warning restore IL2073
        }
    }

    private static OperationHandlerDescriptor[] CreateHandlerDescriptors( [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type handler, object? instance = null )
    {
        ArgumentNullException.ThrowIfNull( handler );

        return [ .. handler.FindInterfaces( HandlerInterfaceFilter, default ).Select(
            ([DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] definition) => OperationHandlerDescriptor.Create(
                definition.GetTypeInfo(),
                handler,
                instance) ) ];
    }

    private static IEnumerable<OperationHandlerDescriptor> FilterDuplicates( IServiceCollection services, IEnumerable<OperationHandlerDescriptor> descriptors )
    {
        ArgumentNullException.ThrowIfNull( descriptors );
        return descriptors.Where( descriptor => !services.Any( service => service.ImplementationInstance is OperationHandlerDescriptor existing && existing == descriptor ) );
    }

    private static bool HandlerInterfaceFilter( Type type, object? state )
    {
        ArgumentNullException.ThrowIfNull( type );
        if( !type.IsGenericType )
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        if( !(definition == typeof( IOperationHandler<> ) || definition == typeof( IOperationHandler<,> )) )
        {
            return false;
        }

        if( state is Type operation )
        {
            return type.GenericTypeArguments[ 0 ] == operation;
        }

        return true;
    }
}

/// <summary> Represents the metadata of an <see cref="IOperationHandler{T}"/> implementation. </summary>
/// <param name="DefinitionType"> The (interface) definition of the handler implementation. </param>
[ImmutableObject( true )]
public sealed record class OperationHandlerDescriptor( Type DefinitionType )
{
    /// <summary> The type of the handler implementation. </summary>
    [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )]
    public Type HandlerType { get; internal init; }

    /// <summary> A singleton instance of the handler. </summary>
    public object? Instance { get; internal init; }

    /// <summary> A compiled delegate of the <see cref="IOperationHandler{T}.Invoke(T, CancellationToken)"/> method. </summary>
    public Delegate Invoke { get; internal init; }

    /// <summary> The type of <see cref="IOperation"/> handled by the implementation. </summary>
    public Type OperationType { get; internal init; }

    internal static OperationHandlerDescriptor Create(
        [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicMethods )] Type definition,
        [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] Type implementation,
        object? instance )
    {
        ArgumentNullException.ThrowIfNull( definition );

        var operationType = definition.GenericTypeArguments[ 0 ].GetTypeInfo();
        return new( definition )
        {
            HandlerType = implementation,
            Instance = instance,
            Invoke = CreateDelegate( definition ),
            OperationType = operationType.IsGenericType ? operationType.GetGenericTypeDefinition().GetTypeInfo() : operationType,
        };

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        [SuppressMessage( "AOT", "IL3050", Justification = "Referenced types are guaranteed at runtime by the generic type definitions on the underlying HandlerType." )]
        static Delegate CreateDelegate( [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicMethods )] Type type )
        {
            ArgumentNullException.ThrowIfNull( type );

            var method = type.GetMethod( "Invoke", BindingFlags.Instance | BindingFlags.Public ) ?? throw new MissingMethodException( $"Type '{type.FullName}' is missing method 'Invoke'. This may be the result of code trimming." );
            return method.CreateDelegate(
                typeof( Func<,,,> ).GetTypeInfo().MakeGenericType(
                    type,
                    type.GenericTypeArguments[ 0 ].GetTypeInfo(),
                    typeof( CancellationToken ),
                    method.ReturnType ) );
        }
    }
}