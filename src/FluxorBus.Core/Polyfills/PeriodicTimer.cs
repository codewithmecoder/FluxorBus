#if NETSTANDARD2_0
namespace System.Threading;

/// <summary>
/// Polyfill of <see cref="System.Threading.PeriodicTimer"/> for netstandard2.0.
/// Fires once per period using <see cref="Task.Delay(TimeSpan, CancellationToken)"/>.
/// </summary>
internal sealed class PeriodicTimer : IDisposable
{
    private readonly TimeSpan _period;
    private bool _disposed;

    public PeriodicTimer(TimeSpan period)
    {
        if (period <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(period));
        _period = period;
    }

    public async System.Threading.Tasks.ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return false;
        try
        {
            await System.Threading.Tasks.Task.Delay(_period, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    public void Dispose() => _disposed = true;
}
#endif