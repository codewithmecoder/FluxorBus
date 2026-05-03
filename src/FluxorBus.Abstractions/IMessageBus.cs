namespace FluxorBus.Abstractions;

/// <summary>
/// Defines a contract for publishing messages asynchronously to a message bus.
/// </summary>
/// <remarks>Implementations of this interface are responsible for delivering messages to registered handlers or
/// subscribers. Thread safety and delivery guarantees may vary depending on the implementation.</remarks>
public interface IMessageBus
{
    /// <summary>
    /// Publishes a message to the message bus asynchronously. The message will be delivered to all registered handlers
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask PublishAsync<TMessage>(TMessage message, CancellationToken ct = default)
        where TMessage : IMessage;
}