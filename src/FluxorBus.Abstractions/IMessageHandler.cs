namespace FluxorBus.Abstractions;

/// <summary>
/// Defines a contract for handling messages of a specified type asynchronously.
/// </summary>
/// <remarks>Implement this interface to process messages in an asynchronous manner. The handler may be used in
/// messaging or event-driven systems to encapsulate message-specific processing logic.</remarks>
/// <typeparam name="TMessage">The type of message to handle. Must implement the <see cref="IMessage"/> interface.</typeparam>
public interface IMessageHandler<in TMessage>
    where TMessage : IMessage
{
    /// <summary>
    /// Asynchronously handles the specified message.
    /// </summary>
    /// <param name="message">The message to be processed. Cannot be null.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task HandleAsync(TMessage message, CancellationToken ct);
}