using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluxorBus.Core;
using FluxorBus.Core.Channel;

namespace FluxorBus.Benchmark;

/// <summary>
/// Benchmarks <see cref="ChannelMessageBus"/> in isolation (no pipeline, no DI scope) to establish
/// a baseline for raw channel write performance.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(iterationCount: 3, warmupCount: 1)]
public class ChannelMessageBusBenchmark
{
    private ChannelMessageBus _bus = null!;
    private OrderCreated _message = null!;

    [Params(1, 1_000, 10_000)]
    public int MessageCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _bus = new ChannelMessageBus(new FluxorBusOptions { Capacity = 100_000 });
        _message = new OrderCreated(Guid.NewGuid(), 1.0m);

        // Drain the channel in the background so it never fills up
        _ = Task.Run(async () =>
        {
            await foreach (var _ in _bus.ReadAllAsync(CancellationToken.None)) { }
        });
    }

    [Benchmark(Description = "ChannelMessageBus.PublishAsync – raw channel write")]
    public async Task RawPublish()
    {
        for (var i = 0; i < MessageCount; i++)
            await _bus.PublishAsync(_message);
    }
}
