using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluxorBus.Core.Execution;
using FluxorBus.DependencyInjection;
using FluxorBus.SourceGen;
using Microsoft.Extensions.DependencyInjection;

namespace FluxorBus.Benchmark;

/// <summary>
/// Benchmarks the <see cref="MessageExecutor{TMessage}"/> directly, isolating handler + pipeline
/// execution cost from channel I/O.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(iterationCount: 3, warmupCount: 1)]
public class MessageExecutorBenchmark
{
    private IServiceScope _scope = null!;
    private MessageExecutor<OrderCreated> _executor = null!;
    private OrderCreated _message = null!;
    private ServiceProvider _provider = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFluxorBus();
        services.AddFluxorBusGenerated();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _executor = _scope.ServiceProvider.GetRequiredService<MessageExecutor<OrderCreated>>();
        _message = new OrderCreated(Guid.NewGuid(), 42m);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        _scope.Dispose();
        await _provider.DisposeAsync();
    }

    [Benchmark(Description = "Execute – single handler, full pipeline")]
    public Task ExecuteSingleMessage() => _executor.Execute(_message, CancellationToken.None);
}
