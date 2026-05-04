using FluxorBus.Abstractions;
using FluxorBus.Core;

namespace FluxorBus.Pipeline;

/// <summary>
/// Provides a pipeline behavior that automatically retries message handling operations when exceptions occur.
/// </summary>
/// <remarks>This behavior attempts to execute the next handler in the pipeline up to <see cref="FluxorBusOptions.RetryAttempts"/>
/// times if exceptions are thrown, introducing a configurable delay between retries. It is typically used to improve
/// resilience in message processing scenarios where transient failures may occur.</remarks>
/// <typeparam name="T">The type of the message being handled by the pipeline behavior.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="RetryBehavior{T}"/> using the provided options.
/// </remarks>
/// <param name="options">The FluxorBus options containing retry configuration.</param>
public class RetryBehavior<T>(FluxorBusOptions options) : IPipelineBehavior<T>
{
    private readonly int _retryAttempts = options.RetryAttempts;
    private readonly int _retryDelayMilliseconds = options.RetryDelayMilliseconds;

    /// <summary>
    /// Invokes the next message handler in the pipeline with retry logic for transient failures.
    /// </summary>
    /// <remarks>If the handler throws an exception, the operation is retried up to <see cref="FluxorBusOptions.RetryAttempts"/>
    /// additional times with a linear back-off delay between attempts. The operation can be canceled by the provided
    /// cancellation token.</remarks>
    /// <param name="message">The message to be processed by the handler.</param>
    /// <param name="next">A delegate representing the next handler to invoke in the pipeline.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(T message, MessageHandlerDelegate next, CancellationToken ct)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                await next();
                return;
            }
            catch when (++attempt < _retryAttempts)
            {
                await Task.Delay(_retryDelayMilliseconds * attempt, ct);
            }
        }
    }
}