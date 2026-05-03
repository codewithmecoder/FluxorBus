using FluxorBus.Abstractions;
using FluxorBus.SourceGen;

namespace FluxorBus.SampleApi.Features.Orders;

[MessageHandler]
public sealed class OrderCreatedHandler(ILogger<OrderCreatedHandler> logger) : IMessageHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated message, CancellationToken ct)
    {
        logger.LogError("[DB] Saving order {OrderId} Amount: {Amount}, {Ts}",
            message.OrderId, message.Amount, DateTime.Now.Ticks);
        return Task.CompletedTask;
    }
}