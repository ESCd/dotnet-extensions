namespace ESCd.Extensions.OperationInvoker.Abstractions;

/// <summary> Describes a type that can invoke an <see cref="IOperation"/>. </summary>
public interface IOperationInvoker
{
    /// <summary> Invoke an operation. </summary>
    /// <param name="operation"> The operation to invoke. </param>
    Task Invoke( IOperation operation, CancellationToken cancellation = default );

    /// <summary> Invoke an operation. </summary>
    /// <param name="operation"> The operation to invoke. </param>
    Task<TResult> Invoke<TResult>( IOperation<TResult> operation, CancellationToken cancellation = default );
}