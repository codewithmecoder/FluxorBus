using FluxorBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FluxorBus.Core.Execution;

/// <summary>
/// Executes all registered batch handlers for messages of type <typeparamref name="TMessage"/>.
/// </summary>
/// <remarks>
/// Messages are cast to <typeparamref name="TMessage"/> and dispatched together as a single list
/// to every registered <see cref="IMessageBatchHandler{TMessage}"/>, enabling bulk processing
/// such as batched database inserts.
/// </remarks>
/// <typeparam name="TMessage">The type of message to be processed in batch. Must implement <see cref="IMessageBatch"/>.</typeparam>
/// <param name="sp">The scoped service provider used to resolve batch handlers at execution time.</param>
public class MessageBatchExecutor<TMessage>(IServiceProvider sp) : IMessageBatchExecutor
    where TMessage : IMessageBatch
{
    /// <inheritdoc />
    public async Task Execute(IReadOnlyList<object> messages, CancellationToken ct)
    {
        if (messages.Count == 0)
            return;

        var typed = messages.Select(m => (TMessage)m).ToList();

        var handlers = sp.GetServices<IMessageBatchHandler<TMessage>>().ToArray();

        foreach (var handler in handlers)
            await handler.HandleAsync(typed, ct);
    }
}
