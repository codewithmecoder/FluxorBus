using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluxorBus.Abstractions;
using FluxorBus.Core.Channel;
using FluxorBus.DependencyInjection;
using FluxorBus.SourceGen;
using Microsoft.Extensions.DependencyInjection;

namespace FluxorBus.Benchmark;

/// <summary>
/// Benchmarks DI scope creation and <see cref="IMessageBus"/> resolution cost per publish cycle,
/// simulating the per-request pattern typical in ASP.NET Core controllers.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(iterationCount: 3, warmupCount: 1)]
public class DiScopeBenchmark
{
    private ServiceProvider _provider = null!;
    private CancellationTokenSource _drainerCts = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFluxorBus(opt => opt.Capacity = 100_000);
        services.AddFluxorBusGenerated();
        _provider = services.BuildServiceProvider();

        // The hosted MessageConsumer is not started in a bare ServiceProvider, so the
        // bounded channel would fill up and cause WriteAsync to hang indefinitely.
        // Run a background drainer to keep the channel empty throughout the benchmark.
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

    [Benchmark(Description = "Resolve IMessageBus from root provider")]
    public IMessageBus ResolveFromRoot()
        => _provider.GetRequiredService<IMessageBus>();

    [Benchmark(Description = "Create scope + resolve + publish + dispose")]
    public async Task ScopeCreateAndPublish()
    {
        await using var scope = _provider.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.PublishAsync(new OrderCreated(Guid.NewGuid(), 10m));
    }
}
