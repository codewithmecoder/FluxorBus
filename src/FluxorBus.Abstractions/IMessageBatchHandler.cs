namespace FluxorBus.Abstractions;

/// <summary>
/// Defines a contract for handling a batch of messages of a specified type asynchronously.
/// </summary>
/// <remarks>
/// Implement this interface to process multiple messages together in a single invocation,
/// enabling efficient bulk operations such as batch database writes.
/// The message type must implement <see cref="IMessageBatch"/> to participate in batch collection.
/// </remarks>
/// <typeparam name="TMessage">The type of message to handle. Must implement <see cref="IMessageBatch"/>.</typeparam>
public interface IMessageBatchHandler<in TMessage>
    where TMessage : IMessageBatch
{
    /// <summary>
    /// Asynchronously handles a batch of messages.
    /// </summary>
    /// <param name="messages">The accumulated list of messages to process. Never null or empty.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task HandleAsync(IReadOnlyList<TMessage> messages, CancellationToken ct);
}
