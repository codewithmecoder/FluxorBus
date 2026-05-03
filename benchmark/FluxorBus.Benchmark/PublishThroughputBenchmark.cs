using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluxorBus.Abstractions;
using FluxorBus.Core.Channel;
using FluxorBus.DependencyInjection;
using FluxorBus.SourceGen;
using Microsoft.Extensions.DependencyInjection;

namespace FluxorBus.Benchmark;

/// <summary>
/// Benchmarks the end-to-end PublishAsync throughput of <see cref="IMessageBus"/>.
/// The hosted consumer is intentionally NOT started so we measure pure channel-write
/// latency without handler execution noise.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(iterationCount: 3, warmupCount: 1)]
public class PublishThroughputBenchmark
{
    private IMessageBus _bus = null!;
    private ServiceProvider _provider = null!;
    private CancellationTokenSource _drainerCts = null!;

    [Params(1, 100, 1_000)]
    public int MessageCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFluxorBus(opt => opt.Capacity = 100_000);
        services.AddFluxorBusGenerated();
        _provider = services.BuildServiceProvider();
        _bus = _provider.GetRequiredService<IMessageBus>();

        // MessageConsumer hosted service is not started in a bare ServiceProvider.
        // Without a drainer the bounded channel fills up and WriteAsync blocks forever.
        _drainerCts = new CancellationTokenSource();
        var channelBus = _provider.GetRequiredService<ChannelMessageBus>();
        var ct = _drainerCts.Token;
        _ = Task.Run(async () =>
        {
            await foreach (var _ in channelBus.ReadAllAsync(ct)) { }
        }, ct);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _drainerCts.CancelAsync();
        _drainerCts.Dispose();
        await _provider.DisposeAsync();
    }

    [Benchmark(Description = "PublishAsync – fire N messages")]
    public async Task PublishMessages()
    {
        var msg = new OrderCreated(Guid.NewGuid(), 99.99m);
        for (var i = 0; i < MessageCount; i++)
            await _bus.PublishAsync(msg);
    }
}
