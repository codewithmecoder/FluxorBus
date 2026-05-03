namespace FluxorBus.Abstractions;

/// <summary>
/// Represents an asynchronous method that handles a message without input parameters or a return value.
/// </summary>
/// <remarks>Use this delegate to define message handling logic that executes asynchronously. The returned task
/// should complete when the message processing is finished.</remarks>
/// <returns>A task that represents the asynchronous operation.</returns>
public delegate Task MessageHandlerDelegate();

/// <summary>
/// Defines a behavior that can be executed as part of a message handling pipeline.
/// </summary>
/// <remarks>Implementations of this interface can be used to add cross-cutting concerns, such as logging,
/// validation, or exception handling, to the message processing pipeline. Each behavior can perform actions before
/// and/or after invoking the next delegate in the pipeline.</remarks>
/// <typeparam name="TMessage">The type of the message to be processed by the pipeline behavior.</typeparam>
public interface IPipelineBehavior<in TMessage>
{
    /// <summary>
    /// Processes the specified message asynchronously and optionally invokes the next handler in the pipeline.
    /// </summary>
    /// <remarks>Implementations can perform custom processing before or after invoking the next handler. If
    /// the next delegate is not called, message processing may be short-circuited.</remarks>
    /// <param name="message">The message to be handled. Cannot be null.</param>
    /// <param name="next">A delegate representing the next handler to invoke in the processing pipeline. This delegate should be called to
    /// continue message processing.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous message handling operation.</returns>
    Task HandleAsync(TMessage message, MessageHandlerDelegate next, CancellationToken ct);
}