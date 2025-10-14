using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ESCd.Extensions.OperationInvoker;

internal sealed class HandlerDescriptorResolver( IEnumerable<OperationHandlerDescriptor> descriptors )
{
    private readonly ConcurrentDictionary<Type, OperationHandlerDescriptor> descriptorsByOperationType = ToDictionary( descriptors );

    public OperationHandlerDescriptor? Resolve( Type operationType )
    {
        operationType = operationType.GetTypeInfo();
        if( descriptorsByOperationType.TryGetValue( operationType, out var descriptor ) )
        {
            return descriptor;
        }

        if( operationType.IsGenericType )
        {
            var genericType = operationType.GetGenericTypeDefinition().GetTypeInfo();
            if( descriptorsByOperationType.TryGetValue( genericType, out descriptor ) )
            {
                return descriptorsByOperationType.GetOrAdd(
                    operationType.GetTypeInfo(),
                    ( [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All )] type ) =>
                    {
#pragma warning disable IL2055,IL3050
                        var handler = descriptor.HandlerType.MakeGenericType( type.GenericTypeArguments );
#pragma warning restore IL2055,IL3050

                        return OperationServiceExtensions.CreateHandlerDescriptor( handler, type, descriptor.Instance );
                    } );
            }
        }

        return default;
    }

    private static ConcurrentDictionary<Type, OperationHandlerDescriptor> ToDictionary( IEnumerable<OperationHandlerDescriptor> descriptors )
    {
        var data = new Dictionary<Type, OperationHandlerDescriptor>();
        foreach( var descriptor in descriptors )
        {
            data[ descriptor.OperationType ] = descriptor;
        }

        return new( data );
    }
}