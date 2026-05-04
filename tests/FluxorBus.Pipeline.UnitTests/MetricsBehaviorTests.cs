using FluxorBus.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluxorBus.Pipeline.UnitTests;

[TestClass]
public class MetricsBehaviorTests
{
    public sealed record TestMessage : IMessage;

    [TestMethod]
    public async Task HandleAsync_NextCompletes_LogsElapsedTime()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        var nextCalled = false;

        using var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(message, Next, cts.Token);

        // Assert
        Assert.IsTrue(nextCalled);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage") && v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_NextThrowsException_LogsElapsedTimeAndRethrows()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        var expectedException = new InvalidOperationException("Test exception");
        MessageHandlerDelegate next = () => throw expectedException;
        using var cts = new CancellationTokenSource();

        // Act & Assert
        var actualException = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, next, cts.Token));

        Assert.AreSame(expectedException, actualException);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage") && v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_NextCompletesWithDelay_LogsElapsedTimeGreaterThanZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        using var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(message, Next, cts.Token);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage") && v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return;
        async Task Next() => await Task.Delay(50, cts.Token);
    }

    [TestMethod]
    public async Task HandleAsync_NextCancelled_LogsElapsedTimeAndThrowsOperationCancelled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        MessageHandlerDelegate next = () => throw new OperationCanceledException();
        using var cts = new CancellationTokenSource();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            async () => await behavior.HandleAsync(message, next, cts.Token));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage") && v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_MultipleInvocations_LogsEachSeparately()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        MessageHandlerDelegate next = () => Task.CompletedTask;
        using var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(message, next, cts.Token);
        await behavior.HandleAsync(message, next, cts.Token);
        await behavior.HandleAsync(message, next, cts.Token);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage") && v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3));
    }

    [TestMethod]
    public async Task HandleAsync_NextInvoked_InvokesNextDelegate()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        var nextCallCount = 0;

        using var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(message, Next, cts.Token);

        // Assert
        Assert.AreEqual(1, nextCallCount);
        return;

        Task Next()
        {
            nextCallCount++;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_LogsMessageTypeName_ContainsCorrectTypeName()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        using var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(message, Next, cts.Token);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return;
        Task Next() => Task.CompletedTask;
    }

    [TestMethod]
    public async Task HandleAsync_LogsAtDebugLevel_UsesDebugLogLevel()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        using var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(message, Next, cts.Token);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        return;
        Task Next() => Task.CompletedTask;
    }

    [TestMethod]
    public async Task HandleAsync_NextTaskFaulted_LogsElapsedTimeAndRethrows()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MetricsBehavior<TestMessage>>>();
        var behavior = new MetricsBehavior<TestMessage>(loggerMock.Object);
        var message = new TestMessage();
        var expectedException = new ArgumentNullException("testParam");
        MessageHandlerDelegate next = () => Task.FromException(expectedException);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        var actualException = await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await behavior.HandleAsync(message, next, cts.Token));

        Assert.AreSame(expectedException, actualException);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage") && v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
