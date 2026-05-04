namespace FluxorBus.Core;

/// <summary>
/// Global configuration options for the FluxorBus messaging infrastructure.
/// </summary>
public sealed class FluxorBusOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether batch consuming is enabled.
    /// When <see langword="true"/>, messages are accumulated into batches before processing.
    /// </summary>
    public bool EnableBatchConsume { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of messages in a single batch.
    /// Defaults to <c>64</c>.
    /// </summary>
    public int BatchSize { get; set; } = 64;

    /// <summary>
    /// Gets or sets the maximum time (in milliseconds) to wait before releasing an incomplete batch.
    /// If the batch has not reached <see cref="BatchSize"/> within this window, it is processed anyway.
    /// Defaults to <c>1000</c> ms.
    /// </summary>
    public int BatchTimeReleased { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum number of messages that can be buffered in the channel.
    /// Publishing will wait when the channel is full until space becomes available.
    /// Defaults to <c>10000</c>.
    /// </summary>
    public int Capacity { get; set; } = 10_000;

    /// <summary>
    /// Gets or sets the number of consecutive failures after which the circuit breaker opens.
    /// When the circuit is open, further message handling is blocked until it is reset.
    /// Defaults to <c>5</c>.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for a failing message handler.
    /// The handler is tried once initially, then retried up to this number of additional times.
    /// Defaults to <c>3</c>.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in milliseconds between retry attempts.
    /// The actual delay is multiplied by the current attempt number to produce a linear back-off.
    /// Defaults to <c>50</c> ms.
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 50;
}
