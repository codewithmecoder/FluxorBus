namespace FluxorBus.Core.Execution;

/// <summary>
/// Defines a contract for executing a message asynchronously.
/// </summary>
/// <remarks>Implementations of this interface should process the provided message according to
/// application-specific logic. The execution may be canceled by passing a cancellation token.</remarks>
public interface IMessageExecutor
{
    /// <summary>
    /// Executes an operation using the specified message and observes cancellation requests.
    /// </summary>
    /// <param name="message">The message object to be processed by the operation. Cannot be null.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous execution of the operation.</returns>
    Task Execute(object message, CancellationToken ct);
}