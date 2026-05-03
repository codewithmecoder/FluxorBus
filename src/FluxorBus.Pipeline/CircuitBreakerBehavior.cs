using FluxorBus.Abstractions;

namespace FluxorBus.Pipeline;

/// <summary>
/// Provides a pipeline behavior that implements the circuit breaker pattern to prevent repeated execution of failing
/// operations.
/// </summary>
/// <remarks>The circuit breaker automatically opens after a specified number of consecutive failures, temporarily
/// blocking further message handling until reset. This helps to prevent system overload and allows dependent services
/// time to recover. Thread safety is not guaranteed; use with care in multi-threaded scenarios.</remarks>
/// <typeparam name="T">The type of message handled by the pipeline behavior.</typeparam>
public class CircuitBreakerBehavior<T> : IPipelineBehavior<T>
{
    private int _failures;
    private bool _open;

    /// <summary>
    /// Processes the specified message asynchronously, invoking the next handler in the pipeline unless the circuit is
    /// open.
    /// </summary>
    /// <remarks>If the handler encounters more than five consecutive failures, the circuit is opened and
    /// subsequent calls will throw an exception until the circuit is reset. This mechanism is intended to prevent
    /// repeated processing failures.</remarks>
    /// <param name="message">The message to be processed by the handler.</param>
    /// <param name="next">A delegate representing the next handler to invoke in the processing pipeline.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown if the circuit is open and message processing is not allowed.</exception>
    public async Task HandleAsync(T message, MessageHandlerDelegate next, CancellationToken ct)
    {
        if (_open)
            throw new Exception("Circuit Open");

        try
        {
            await next();
            _failures = 0;
        }
        catch
        {
            _failures++;

            if (_failures > 5)
                _open = true;

            throw;
        }
    }
}