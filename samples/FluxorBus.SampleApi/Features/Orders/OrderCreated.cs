using FluxorBus.Abstractions;

namespace FluxorBus.SampleApi.Features.Orders;

public record OrderCreated(Guid OrderId, decimal Amount) : IMessage;