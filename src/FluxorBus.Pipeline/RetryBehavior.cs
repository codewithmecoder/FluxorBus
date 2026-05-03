using FluxorBus.Abstractions;

namespace FluxorBus.Pipeline;

/// <summary>
/// Provides a pipeline behavior that automatically retries message handling operations when exceptions occur.
/// </summary>
/// <remarks>This behavior attempts to execute the next handler in the pipeline up to three times if exceptions
/// are thrown, introducing a delay between retries. It is typically used to improve resilience in message processing
/// scenarios where transient failures may occur.</remarks>
/// <typeparam name="T">The type of the message being handled by the pipeline behavior.</typeparam>
public class RetryBehavior<T> : IPipelineBehavior<T>
{
    /// <summary>
    /// Invokes the next message handler in the pipeline with retry logic for transient failures.
    /// </summary>
    /// <remarks>If the handler throws an exception, the operation is retried up to two additional times with
    /// a delay between attempts. The delay increases with each retry. The operation can be canceled by the provided
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
            catch when (++attempt < 3)
            {
                await Task.Delay(50 * attempt, ct);
            }
        }
    }
}