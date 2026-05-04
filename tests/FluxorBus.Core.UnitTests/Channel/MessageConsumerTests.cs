using FluxorBus.Core.Channel;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluxorBus.Core.UnitTests.Channel;

[TestClass]
public class MessageConsumerTests
{
    private Mock<ILogger<MessageConsumer>> _mockLogger = null!;
    private ChannelMessageBus _bus = null!;
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private FluxorBusOptions _options = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockLogger = new Mock<ILogger<MessageConsumer>>();
        _options = new FluxorBusOptions();
        _bus = new ChannelMessageBus(_options);
        _mockServiceProvider = new Mock<IServiceProvider>();
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenEnableBatchConsumeIsTrue_ExecutesWithoutError()
    {
        // Arrange
        _options.EnableBatchConsume = true;
        
        var cancellationTokenSource = new CancellationTokenSource();
        
        var consumer = new TestableMessageConsumer(
            _mockLogger.Object,
            _bus,
            _mockServiceProvider.Object,
            _options);

        // Act & Assert - the method should handle cancellation gracefully
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(50));
        
        await consumer.ExecuteAsyncPublic(cancellationTokenSource.Token);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenEnableBatchConsumeIsFalse_ExecutesWithoutError()
    {
        // Arrange
        _options.EnableBatchConsume = false;
        
        var cancellationTokenSource = new CancellationTokenSource();
        
        var consumer = new TestableMessageConsumer(
            _mockLogger.Object,
            _bus,
            _mockServiceProvider.Object,
            _options);

        // Act
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(50));
        
        try
        {
            await consumer.ExecuteAsyncPublic(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation token is triggered during async iteration
        }
        
        // Assert - if we get here without other exceptions, the test passes
    }

    private class TestableMessageConsumer : MessageConsumer
    {
        public TestableMessageConsumer(
            ILogger<MessageConsumer> logger,
            ChannelMessageBus bus,
            IServiceProvider sp,
            FluxorBusOptions options)
            : base(logger, bus, sp, options)
        {
        }

        public Task ExecuteAsyncPublic(CancellationToken stoppingToken)
        {
            return ExecuteAsync(stoppingToken);
        }
    }
}
