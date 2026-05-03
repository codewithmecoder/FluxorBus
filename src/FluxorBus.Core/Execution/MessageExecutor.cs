using FluxorBus.Abstractions;

namespace FluxorBus.Core.Execution;

/// <summary>
/// Executes all registered handlers and behaviors for messages of type TMessage.
/// </summary>
/// <remarks>Handlers and behaviors are invoked in the order they are registered. This class enables asynchronous
/// execution of message processing pipelines. If a cancellation token is triggered, execution may be stopped before all
/// handlers complete.</remarks>
/// <typeparam name="TMessage">The type of message to be processed. Must implement IMessage.</typeparam>
/// <param name="registry">The registry containing handlers and behaviors to be executed for messages of type TMessage.</param>
public class MessageExecutor<TMessage>(MessageHandlerRegistry<TMessage> registry) : IMessageExecutor
    where TMessage : IMessage
{
    /// <summary>
    /// Executes all registered handlers and behaviors for the specified message asynchronously.
    /// </summary>
    /// <remarks>Handlers and behaviors are invoked in the order registered. If the cancellation token is
    /// triggered, execution may be stopped before all handlers complete.</remarks>
    /// <param name="message">The message to process. Must be of type TMessage.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous execution of all handlers and behaviors.</returns>
    public async Task Execute(object message, CancellationToken ct)
    {
        var msg = (TMessage)message;

        var handlers = registry.Handlers;
        var behaviors = registry.Behaviors;

        foreach (var handler in handlers)
        {
            var handler1 = handler;
            MessageHandlerDelegate next = () => handler1.HandleAsync(msg, ct);

            for (var b = behaviors.Length - 1; b >= 0; b--)
            {
                var behavior = behaviors[b];
                var local = next;
                next = () => behavior.HandleAsync(msg, local, ct);
            }

            await next();
        }
    }
}