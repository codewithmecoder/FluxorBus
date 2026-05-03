using FluxorBus.Abstractions;

namespace FluxorBus.Core.Execution;

/// <summary>
/// Provides a registry for associating message handlers and pipeline behaviors with a specific message type.
/// </summary>
/// <remarks>This class is typically used to organize and retrieve handlers and behaviors for a given message type
/// within a messaging or mediator framework. The order of handlers and behaviors in the arrays determines the order in
/// which they are invoked.</remarks>
/// <typeparam name="TMessage">The type of message that the handlers and behaviors process. Must implement the IMessage interface.</typeparam>
/// <param name="handlers">An array of message handlers responsible for processing messages of type TMessage. Cannot be null.</param>
/// <param name="behaviors">An array of pipeline behaviors that are applied to the processing of messages of type TMessage. Cannot be null.</param>
public sealed class MessageHandlerRegistry<TMessage>(
    IMessageHandler<TMessage>[] handlers,
    IPipelineBehavior<TMessage>[] behaviors)
    where TMessage : IMessage
{
    /// <summary>
    /// Gets the collection of message handlers associated with the current instance.
    /// </summary>
    public IMessageHandler<TMessage>[] Handlers { get; } = handlers;

    /// <summary>
    /// Gets the collection of pipeline behaviors associated with the current instance.
    /// These behaviors are typically executed in a specific order around the message handlers,
    /// allowing for cross-cutting concerns such as logging, validation,
    /// or transaction management to be applied consistently across all handlers for the message type.
    /// </summary>
    public IPipelineBehavior<TMessage>[] Behaviors { get; } = behaviors;
}