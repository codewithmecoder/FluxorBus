using FluxorBus.Abstractions;
using FluxorBus.Core.Execution;
using Moq;

namespace FluxorBus.Core.UnitTests.Execution;

[TestClass]
public class MessageBatchExecutorTests
{
    public class TestMessageBatch : IMessageBatch;

    [TestMethod]
    public async Task Execute_EmptyMessageList_ReturnsEarlyWithoutCallingHandlers()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var executor = new MessageBatchExecutor<TestMessageBatch>(mockServiceProvider.Object);
        var messages = Array.Empty<object>();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(messages, ct);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetService(It.IsAny<Type>()), Times.Never);
    }

    [TestMethod]
    public async Task Execute_NoHandlersRegistered_CompletesSuccessfully()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageBatchHandler<TestMessageBatch>>)))
            .Returns(Array.Empty<IMessageBatchHandler<TestMessageBatch>>());

        var executor = new MessageBatchExecutor<TestMessageBatch>(mockServiceProvider.Object);
        var messages = new List<object> { new TestMessageBatch() };
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(messages, ct);

        // Assert
        // No exception means success
    }

    [TestMethod]
    public async Task Execute_SingleHandler_CallsHandlerWithTypedMessages()
    {
        // Arrange
        var mockHandler = new Mock<IMessageBatchHandler<TestMessageBatch>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageBatchHandler<TestMessageBatch>>)))
            .Returns(new[] { mockHandler.Object });

        var executor = new MessageBatchExecutor<TestMessageBatch>(mockServiceProvider.Object);
        var message1 = new TestMessageBatch();
        var message2 = new TestMessageBatch();
        var messages = new List<object> { message1, message2 };
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(messages, ct);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(
            It.Is<IReadOnlyList<TestMessageBatch>>(list => list.Count == 2 && list[0] == message1 && list[1] == message2),
            ct), Times.Once);
    }

    [TestMethod]
    public async Task Execute_MultipleHandlers_CallsAllHandlersInOrder()
    {
        // Arrange
        var callOrder = new List<int>();
        var mockHandler1 = new Mock<IMessageBatchHandler<TestMessageBatch>>();
        mockHandler1
            .Setup(h => h.HandleAsync(It.IsAny<IReadOnlyList<TestMessageBatch>>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add(1))
            .Returns(Task.CompletedTask);

        var mockHandler2 = new Mock<IMessageBatchHandler<TestMessageBatch>>();
        mockHandler2
            .Setup(h => h.HandleAsync(It.IsAny<IReadOnlyList<TestMessageBatch>>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add(2))
            .Returns(Task.CompletedTask);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageBatchHandler<TestMessageBatch>>)))
            .Returns(new[] { mockHandler1.Object, mockHandler2.Object });

        var executor = new MessageBatchExecutor<TestMessageBatch>(mockServiceProvider.Object);
        var messages = new List<object> { new TestMessageBatch() };
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(messages, ct);

        // Assert
        Assert.HasCount(2, callOrder);
        Assert.AreEqual(1, callOrder[0]);
        Assert.AreEqual(2, callOrder[1]);
        mockHandler1.Verify(h => h.HandleAsync(It.IsAny<IReadOnlyList<TestMessageBatch>>(), ct), Times.Once);
        mockHandler2.Verify(h => h.HandleAsync(It.IsAny<IReadOnlyList<TestMessageBatch>>(), ct), Times.Once);
    }

    [TestMethod]
    public async Task Execute_WithCancellationToken_PassesTokenToHandlers()
    {
        // Arrange
        var mockHandler = new Mock<IMessageBatchHandler<TestMessageBatch>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageBatchHandler<TestMessageBatch>>)))
            .Returns(new[] { mockHandler.Object });

        var executor = new MessageBatchExecutor<TestMessageBatch>(mockServiceProvider.Object);
        var messages = new List<object> { new TestMessageBatch() };
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        // Act
        await executor.Execute(messages, ct);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(It.IsAny<IReadOnlyList<TestMessageBatch>>(), ct), Times.Once);
    }

    [TestMethod]
    public async Task Execute_SingleMessage_CreatesListWithOneElement()
    {
        // Arrange
        var mockHandler = new Mock<IMessageBatchHandler<TestMessageBatch>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageBatchHandler<TestMessageBatch>>)))
            .Returns(new[] { mockHandler.Object });

        var executor = new MessageBatchExecutor<TestMessageBatch>(mockServiceProvider.Object);
        var message = new TestMessageBatch();
        var messages = new List<object> { message };
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(messages, ct);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(
            It.Is<IReadOnlyList<TestMessageBatch>>(list => list.Count == 1 && list[0] == message),
            ct), Times.Once);
    }

    [TestMethod]
    public async Task Execute_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Handler failed");
        var mockHandler = new Mock<IMessageBatchHandler<TestMessageBatch>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<IReadOnlyList<TestMessageBatch>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageBatchHandler<TestMessageBatch>>)))
            .Returns(new[] { mockHandler.Object });

        var executor = new MessageBatchExecutor<TestMessageBatch>(mockServiceProvider.Object);
        var messages = new List<object> { new TestMessageBatch() };
        var ct = CancellationToken.None;

        // Assert
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(Act);
        Assert.AreEqual("Handler failed", exception.Message);
        return;

        // Act
        async Task Act() => await executor.Execute(messages, ct);
    }
}
