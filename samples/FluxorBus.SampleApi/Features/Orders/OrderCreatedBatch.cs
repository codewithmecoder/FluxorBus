using FluxorBus.Abstractions;
using FluxorBus.SourceGen;

namespace FluxorBus.SampleApi.Features.Orders;

public record OrderCreatedBatch(Guid OrderId, decimal Amount) : IMessageBatch;

[MessageHandler]
public sealed class OrderCreatedBatchHandler(ILogger<OrderCreatedBatchHandler> logger) : IMessageBatchHandler<OrderCreatedBatch>
{
    public Task HandleAsync(IReadOnlyList<OrderCreatedBatch> messages, CancellationToken ct)
    {
        // Simulate batch processing, e.g. bulk insert into database
        logger.LogError("Processing batch of {Count} order created events:", messages.Count);
        foreach (var msg in messages)
        {
            logger.LogError("- OrderId: {OrderId}, Amount: {Amount}", msg.OrderId, msg.Amount);
        }
        return Task.CompletedTask;
    }
}