using FluxorBus.Abstractions;

namespace FluxorBus.Benchmark;

// --- Single message ---
public record OrderCreated(Guid OrderId, decimal Amount) : IMessage;

// --- Batch message ---
public record OrderShipped(Guid OrderId) : IMessageBatch;
