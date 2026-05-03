using FluxorBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FluxorBus.Core.Execution;

/// <summary>
/// Factory responsible for creating and managing message handler registries. The MessageHandlerRegistryFactory is used to
/// encapsulate the creation logic and ensure that registries are properly initialized with their respective handlers and behaviors.
/// </summary>
/// <param name="sp">The service provider used to resolve message handlers and pipeline behaviors.</param>
public class MessageHandlerRegistryFactory(IServiceProvider sp)
{
    /// <summary>
    /// Creates a new instance of the message handler registry for the specified message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message for which handlers and pipeline behaviors are to be registered. Must implement the IMessage
    /// interface.</typeparam>
    /// <returns>A MessageHandlerRegistry<TMessage/> containing all registered handlers and pipeline behaviors for the specified
    /// message type.</returns>
    public MessageHandlerRegistry<TMessage> Create<TMessage>()
        where TMessage : IMessage
    {
        var handlers = sp.GetServices<IMessageHandler<TMessage>>().ToArray();
        var behaviors = sp.GetServices<IPipelineBehavior<TMessage>>().ToArray();

        return new MessageHandlerRegistry<TMessage>(handlers, behaviors);
    }
}