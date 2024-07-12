namespace ESCd.Extensions.OperationInvoker.Abstractions;

/// <summary> Describes an operation that does not return a result. </summary>
public interface IOperation;

/// <summary> Describes an operation that returns a result of type <typeparamref name="TResult"/>. </summary>
/// <typeparam name="TResult"> The type of the result of invoking the operation. </typeparam>
public interface IOperation<TResult>;