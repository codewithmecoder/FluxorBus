using FluxorBus.Abstractions;
using FluxorBus.SourceGen;

namespace FluxorBus.SampleApi.Features.Orders;

[MessageHandler]
public sealed class SendEmailOnOrderCreated(ILogger<SendEmailOnOrderCreated> logger) : IMessageHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated message, CancellationToken ct)
    {
        logger.LogError("[EMAIL] Sending confirmation for {OrderId}", message.OrderId);
        return Task.CompletedTask;
    }
}