using FluxorBus.Abstractions;
using FluxorBus.SourceGen;

namespace FluxorBus.Benchmark;

[MessageHandler]
public class OrderCreatedHandler : IMessageHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated message, CancellationToken ct) => Task.CompletedTask;
}

[MessageHandler]
public class OrderShippedBatchHandler : IMessageBatchHandler<OrderShipped>
{
    public Task HandleAsync(IReadOnlyList<OrderShipped> messages, CancellationToken ct) => Task.CompletedTask;
}
