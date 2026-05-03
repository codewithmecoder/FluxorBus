using System.Threading.Channels;
using FluxorBus.Abstractions;

namespace FluxorBus.Core.Channel;

/// <summary>
/// Provides an in-memory message bus implementation that uses a bounded channel to publish and consume messages
/// asynchronously.
/// </summary>
/// <remarks>ChannelMessageBus enables asynchronous message passing between producers and consumers using a
/// thread-safe, bounded channel. It is suitable for scenarios where messages need to be buffered and processed
/// concurrently. The channel enforces a maximum capacity, and publishing will wait if the channel is full until space
/// becomes available.</remarks>
/// <param> name="capacity">The maximum number of messages that can be buffered in the channel. Default is 10,000.</param>
public class ChannelMessageBus(int capacity = 10_000) : IMessageBus
{
    private readonly Channel<IMessage> _channel = System.Threading.Channels.Channel.CreateBounded<IMessage>(new BoundedChannelOptions(capacity)
    {
        FullMode = BoundedChannelFullMode.Wait
    });


    /// <inheritdoc />
    public ValueTask PublishAsync<TMessage>(TMessage message, CancellationToken ct = default)
        where TMessage : IMessage
        => _channel.Writer.WriteAsync(message, ct);

    /// <summary>
    /// Reads all messages from the channel as an asynchronous stream. This method will block until a message is available or the channel is completed.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IMessage> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}