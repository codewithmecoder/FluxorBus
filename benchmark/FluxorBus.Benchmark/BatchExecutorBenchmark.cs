using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluxorBus.Abstractions;
using FluxorBus.Core.Execution;
using FluxorBus.DependencyInjection;
using FluxorBus.SourceGen;
using Microsoft.Extensions.DependencyInjection;

namespace FluxorBus.Benchmark;

/// <summary>
/// Benchmarks <see cref="MessageBatchExecutor{TMessage}"/> to measure the overhead of
/// dispatching a batch of <see cref="IMessageBatch"/> messages to batch handlers.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(iterationCount: 3, warmupCount: 1)]
public class BatchExecutorBenchmark
{
    private IServiceScope _scope = null!;
    private MessageBatchExecutor<OrderShipped> _executor = null!;
    private ServiceProvider _provider = null!;

    [Params(8, 32, 64)]
    public int BatchSize { get; set; }

    private IReadOnlyList<object> _batch = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFluxorBus(opt => opt.EnableBatchConsume = true);
        services.AddFluxorBusGenerated();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        _executor = _scope.ServiceProvider.GetRequiredService<MessageBatchExecutor<OrderShipped>>();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _batch = Enumerable.Range(0, BatchSize)
            .Select(object (_) => new OrderShipped(Guid.NewGuid()))
            .ToList();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        _scope.Dispose();
        await _provider.DisposeAsync();
    }

    [Benchmark(Description = "Execute – batch handler dispatch")]
    public Task ExecuteBatch() => _executor.Execute(_batch, CancellationToken.None);
}
