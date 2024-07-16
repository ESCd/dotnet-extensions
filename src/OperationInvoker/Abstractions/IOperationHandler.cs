using System.Diagnostics.CodeAnalysis;

namespace ESCd.Extensions.OperationInvoker.Abstractions;

/// <summary> Describes a type that handles an operation of type <typeparamref name="TOperation"/>. </summary>
/// <typeparam name="TOperation"> The type of <see cref="IOperation"/> handled. </typeparam>
[DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicMethods )]
public interface IOperationHandler<TOperation>
    where TOperation : IOperation
{
    /// <summary> Invokes an operation. </summary>
    /// <param name="operation"> The operation being invoked. </param>
    /// <param name="cancellation"> A token to trigger cancellation of the operation. </param>
    Task Invoke( TOperation operation, CancellationToken cancellation );
}

/// <summary> Describes a type that handles an operation of type <typeparamref name="TOperation"/>, that returns a result of type <typeparamref name="TResult"/>. </summary>
/// <typeparam name="TOperation"> The type of <see cref="IOperation{TResult}"/> handled. </typeparam>
/// <typeparam name="TResult"> The type of the result of invoking the handler. </typeparam>
[DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.PublicMethods )]
public interface IOperationHandler<TOperation, TResult>
    where TOperation : IOperation<TResult>
{
    /// <summary> Invokes an operation. </summary>
    /// <param name="operation"> The operation being invoked. </param>
    /// <param name="cancellation"> A token to trigger cancellation of the operation. </param>
    Task<TResult> Invoke( TOperation operation, CancellationToken cancellation );
}