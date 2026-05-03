using System.Threading.Channels;
using FluxorBus.Abstractions;
using ThreadingChannel = System.Threading.Channels.Channel;

namespace FluxorBus.Core.Channel;

/// <summary>
/// Provides an in-memory message bus implementation that uses a bounded channel to publish and consume messages
/// asynchronously.
/// </summary>
/// <remarks>ChannelMessageBus enables asynchronous message passing between producers and consumers using a
/// thread-safe, bounded channel. It is suitable for scenarios where messages need to be buffered and processed
/// concurrently. The channel enforces a maximum capacity, and publishing will wait if the channel is full until space
/// becomes available.</remarks>
/// <param name="options">The global FluxorBus configuration options. The <see cref="FluxorBusOptions.Capacity"/>
/// value controls the maximum number of messages that can be buffered in the channel.</param>
public class ChannelMessageBus(FluxorBusOptions options) : IMessageBus
{
    private readonly Channel<IMessage> _channel = ThreadingChannel.CreateBounded<IMessage>(new BoundedChannelOptions(options.Capacity)
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

    /// <summary>
    /// Returns the underlying channel reader for low-level read operations such as <see cref="ChannelReader{T}.WaitToReadAsync"/> and <see cref="ChannelReader{T}.TryRead"/>.
    /// </summary>
    public ChannelReader<IMessage> GetReader() => _channel.Reader;
}