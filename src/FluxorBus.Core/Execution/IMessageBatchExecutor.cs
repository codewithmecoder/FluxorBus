namespace FluxorBus.Core.Execution;

/// <summary>
/// Defines a contract for executing a batch of messages of the same type asynchronously.
/// </summary>
public interface IMessageBatchExecutor
{
    /// <summary>
    /// Executes all registered batch handlers for the provided list of messages.
    /// </summary>
    /// <param name="messages">The batch of messages to process. All elements must be of the same concrete type.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    Task Execute(IReadOnlyList<object> messages, CancellationToken ct);
}
