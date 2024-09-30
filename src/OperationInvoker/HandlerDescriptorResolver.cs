namespace ESCd.Extensions.OperationInvoker;

internal sealed class HandlerDescriptorResolver( IEnumerable<OperationHandlerDescriptor> descriptors )
{
    private readonly Dictionary<Type, OperationHandlerDescriptor> descriptorsByOperationType = ToDictionary( descriptors );

    public OperationHandlerDescriptor? Resolve( Type operationType )
    {
        if( descriptorsByOperationType.TryGetValue( operationType, out var descriptor ) )
        {
            return descriptor;
        }

        return default;
    }

    private static Dictionary<Type, OperationHandlerDescriptor> ToDictionary( IEnumerable<OperationHandlerDescriptor> descriptors )
    {
        var descriptorsByOperationType = new Dictionary<Type, OperationHandlerDescriptor>();
        foreach( var descriptor in descriptors )
        {
            descriptorsByOperationType[ descriptor.OperationType ] = descriptor;
        }

        return descriptorsByOperationType;
    }
}