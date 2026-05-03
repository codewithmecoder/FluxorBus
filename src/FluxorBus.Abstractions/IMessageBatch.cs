namespace FluxorBus.Abstractions;

/// <summary>
/// Marker interface that opts a message type into batch handler processing.
/// When a message implements both <see cref="IMessage"/> and <see cref="IMessageBatch"/>,
/// the consumer will accumulate messages of that type and dispatch them as a list
/// to any registered <see cref="IMessageBatchHandler{TMessage}"/>.
/// </summary>
public interface IMessageBatch : IMessage { }
