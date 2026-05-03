using FluxorBus.Abstractions;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FluxorBus.Pipeline;

/// <summary>
/// Provides a pipeline behavior that measures and logs the execution time of message handlers.
/// </summary>
/// <remarks>This behavior is typically used to monitor the performance of message handlers by logging the elapsed
/// time for each handled message. It can be inserted into a pipeline to collect metrics for diagnostic or monitoring
/// purposes.</remarks>
/// <typeparam name="T">The type of message handled by the pipeline behavior.</typeparam>
public class MetricsBehavior<T>(ILogger<MetricsBehavior<T>> logger) : IPipelineBehavior<T>
{
    /// <summary>
    /// Processes the specified message asynchronously and invokes the next handler in the pipeline.
    /// </summary>
    /// <remarks>The elapsed processing time for the message is logged to the console after the handler
    /// completes. This method is typically used in middleware scenarios to measure and log the duration of message
    /// handling.</remarks>
    /// <param name="message">The message to be processed by the handler.</param>
    /// <param name="next">A delegate representing the next handler to invoke in the processing pipeline.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(T message, MessageHandlerDelegate next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await next();
        }
        finally
        {
            logger.LogDebug("{MessageType} {ElapsedMilliseconds}ms", typeof(T).Name, sw.ElapsedMilliseconds);
        }
    }
}