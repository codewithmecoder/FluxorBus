using FluxorBus.Abstractions;
using FluxorBus.Core.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluxorBus.Core.Channel;

/// <summary>
/// Consumes messages from a channel-based message bus and processes them in batches using registered message executors.
/// </summary>
/// <remarks>This background service reads messages continuously from the provided message bus and processes them
/// in batches to improve throughput. Each batch is processed within a new dependency injection scope. The service is
/// intended to be hosted as part of an application's background processing infrastructure.</remarks>
/// <param name="bus">The message bus from which messages are read and consumed.</param>
/// <param name="sp">The service provider used to resolve scoped dependencies for message processing.</param>
/// <param name="factory">The factory responsible for providing message executors based on message type.</param>
public class MessageConsumer(
    ChannelMessageBus bus,
    IServiceProvider sp,
    MessageExecutorFactory factory)
    : BackgroundService
{
    private const int BatchSize = 64;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<IMessage>(BatchSize);

        await foreach (var message in bus.ReadAllAsync(stoppingToken))
        {
            batch.Add(message);

            if (batch.Count < BatchSize) continue;
            await Process(batch, stoppingToken);
            batch.Clear();
        }
    }

    private async Task Process(List<IMessage> batch, CancellationToken ct)
    {
        using var scope = sp.CreateScope();

        var tasks = batch.Select(m =>
        {
            var executor = factory.Get(m.GetType());
            return executor.Execute(m, ct);
        });

        await Task.WhenAll(tasks);
    }
}