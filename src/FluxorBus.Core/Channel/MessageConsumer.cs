using FluxorBus.Abstractions;
using FluxorBus.Core.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FluxorBus.Core.Channel;

/// <summary>
/// Consumes messages from a channel-based message bus and processes them in batches using registered message executors.
/// </summary>
/// <remarks>This background service reads messages continuously from the provided message bus and processes them
/// in batches to improve throughput. Each batch is processed within a new dependency injection scope. The service is
/// intended to be hosted as part of an application's background processing infrastructure.</remarks>
/// <param name="logger"></param>
/// <param name="bus">The message bus from which messages are read and consumed.</param>
/// <param name="sp">The service provider used to resolve scoped dependencies for message processing.</param>
/// <param name="options">The global FluxorBus configuration options.</param>
public class MessageConsumer(
    ILogger<MessageConsumer> logger,
    ChannelMessageBus bus,
    IServiceProvider sp,
    FluxorBusOptions options)
    : BackgroundService
{
    private static readonly ConcurrentDictionary<Type, Type> ExecutorTypeCache = new();
    private static readonly ConcurrentDictionary<Type, Type> BatchExecutorTypeCache = new();

    private const int MaxConcurrency = 10; // tune based on workload

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.EnableBatchConsume)
            await ExecuteBatchAsync(stoppingToken);
        else
            await ExecuteOneByOneAsync(stoppingToken);
    }

    private async Task ExecuteOneByOneAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in bus.ReadAllAsync(stoppingToken))
        {
            await Process([message], stoppingToken);
        }
    }

    private async Task ExecuteBatchAsync(CancellationToken stoppingToken)
    {
        var batch = new List<IMessage>(options.BatchSize);
        var channelReader = bus.GetReader();
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(options.BatchTimeReleased));

        var timerTask = timer.WaitForNextTickAsync(stoppingToken).AsTask();

        while (!stoppingToken.IsCancellationRequested)
        {
            var readTask = channelReader.WaitToReadAsync(stoppingToken).AsTask();

            var completed = await Task.WhenAny(readTask, timerTask);

            if (completed == timerTask)
            {
                // Deadline elapsed — flush whatever is in the batch
                if (batch.Count > 0)
                {
                    await Process(batch, stoppingToken);
                    batch.Clear();
                }

                if (stoppingToken.IsCancellationRequested) break;
                timerTask = timer.WaitForNextTickAsync(stoppingToken).AsTask();
                continue;
            }

            // Drain all currently available messages into the batch
            while (batch.Count < options.BatchSize && channelReader.TryRead(out var msg))
                batch.Add(msg);

            if (batch.Count < options.BatchSize) continue;

            await Process(batch, stoppingToken);
            batch.Clear();
            timerTask = timer.WaitForNextTickAsync(stoppingToken).AsTask();
        }

        // Flush remaining messages on shutdown
        if (batch.Count > 0)
            await Process(batch, stoppingToken);
    }

    private async Task Process(List<IMessage> batch, CancellationToken ct)
    {
        if (batch.Count == 0)
            return;

        using var scope = sp.CreateScope();
        var provider = scope.ServiceProvider;

        // Separate batch messages (IMessageBatch) from regular messages
        var batchGroups = batch
            .OfType<IMessageBatch>()
            .GroupBy(m => m.GetType())
            .ToList();

        var regularMessages = batch
            .Where(m => m is not IMessageBatch)
            .ToList();

        // Dispatch regular messages concurrently, one handler call per message
        if (regularMessages.Count > 0)
        {
            using var semaphore = new SemaphoreSlim(MaxConcurrency);
            // ReSharper disable once AccessToDisposedClosure
            var tasks = regularMessages.Select(m => ProcessMessage(m, provider, semaphore, ct));
            await Task.WhenAll(tasks);
        }

        // Dispatch each group of batch messages to their batch handlers
        foreach (var group in batchGroups)
        {
            await ProcessBatch(group.Key, [.. group], provider, ct);
        }
    }

    private async Task ProcessMessage(
        IMessage message,
        IServiceProvider provider,
        SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);

        try
        {
            var messageType = message.GetType();

            var executorType = ExecutorTypeCache.GetOrAdd(
                messageType,
                static t => typeof(MessageExecutor<>).MakeGenericType(t)
            );

            var executor = (IMessageExecutor)provider.GetRequiredService(executorType);

            await executor.Execute(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{MessageType} | {Message}", message.GetType().Name, message);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ProcessBatch(
        Type messageType,
        IReadOnlyList<object> messages,
        IServiceProvider provider,
        CancellationToken ct)
    {
        try
        {
            var executorType = BatchExecutorTypeCache.GetOrAdd(
                messageType,
                static t => typeof(MessageBatchExecutor<>).MakeGenericType(t)
            );

            var executor = (IMessageBatchExecutor)provider.GetRequiredService(executorType);

            await executor.Execute(messages, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Batch processing failed for {MessageType}", messageType.Name);
        }
    }
}