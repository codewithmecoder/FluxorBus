using FluxorBus.Abstractions;
using FluxorBus.Core;

namespace FluxorBus.Pipeline.UnitTests;

[TestClass]
public class RetryBehaviorTests
{
    private sealed record TestMessage : IMessage;

    [TestMethod]
    public async Task HandleAsync_SuccessOnFirstAttempt_ReturnsWithoutRetry()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 3, RetryDelayMilliseconds = 50 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;

        // Act
        await behavior.HandleAsync(message, Next, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, executionCount);
        return;

        Task Next()
        {
            executionCount++;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_FailsOnceSucceedsOnSecondAttempt_RetriesOnce()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 3, RetryDelayMilliseconds = 10 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;

        // Act
        await behavior.HandleAsync(message, Next, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, executionCount);
        return;

        Task Next()
        {
            executionCount++;
            return executionCount == 1 ? throw new InvalidOperationException("First attempt fails") : Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_FailsTwiceSucceedsOnThirdAttempt_RetriesTwice()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 3, RetryDelayMilliseconds = 10 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;

        // Act
        await behavior.HandleAsync(message, Next, CancellationToken.None);

        // Assert
        Assert.AreEqual(3, executionCount);
        return;

        Task Next()
        {
            executionCount++;
            return executionCount <= 2 ? throw new InvalidOperationException($"Attempt {executionCount} fails") : Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_FailsAllAttempts_ThrowsException()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 3, RetryDelayMilliseconds = 10 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        MessageHandlerDelegate next = () =>
        {
            executionCount++;
            throw new InvalidOperationException("Always fails");
        };

        // Act & Assert
        _ = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => behavior.HandleAsync(message, next, CancellationToken.None));
        Assert.AreEqual(3, executionCount);
    }

    [TestMethod]
    public async Task HandleAsync_WithLinearBackoff_AppliesCorrectDelays()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 3, RetryDelayMilliseconds = 50 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        var delays = new List<long>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var timestamps = new List<long>();

        // Act
        await behavior.HandleAsync(message, Next, CancellationToken.None);

        // Assert
        Assert.AreEqual(3, executionCount);
        for (var i = 1; i < timestamps.Count; i++)
        {
            delays.Add(timestamps[i] - timestamps[i - 1]);
        }

        // First delay should be approximately 50ms (50 * 1)
        Assert.IsTrue(delays[0] >= 40 && delays[0] <= 100, $"First delay was {delays[0]}ms");
        // Second delay should be approximately 100ms (50 * 2)
        Assert.IsTrue(delays[1] >= 90 && delays[1] <= 150, $"Second delay was {delays[1]}ms");
        return;

        Task Next()
        {
            executionCount++;
            timestamps.Add(stopwatch.ElapsedMilliseconds);
            return executionCount < 3 ? throw new InvalidOperationException($"Attempt {executionCount} fails") : Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_CancellationTokenCanceled_ThrowsOperationCanceledException()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 5, RetryDelayMilliseconds = 1000 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        MessageHandlerDelegate next = () =>
        {
            executionCount++;
            throw new InvalidOperationException("Always fails");
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => behavior.HandleAsync(message, next, cts.Token));
        Assert.IsGreaterThanOrEqualTo(executionCount, 1);
    }

    [TestMethod]
    public async Task HandleAsync_ZeroRetryAttempts_ThrowsOnFirstFailure()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 0, RetryDelayMilliseconds = 50 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        MessageHandlerDelegate next = () =>
        {
            executionCount++;
            throw new InvalidOperationException("Fails");
        };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => behavior.HandleAsync(message, next, CancellationToken.None));
        Assert.AreEqual(1, executionCount);
    }

    [TestMethod]
    public async Task HandleAsync_OneRetryAttempt_RetriesOnce()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 1, RetryDelayMilliseconds = 10 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        MessageHandlerDelegate next = () =>
        {
            executionCount++;
            throw new InvalidOperationException("Always fails");
        };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => behavior.HandleAsync(message, next, CancellationToken.None));
        Assert.AreEqual(1, executionCount);
    }

    [TestMethod]
    public async Task HandleAsync_ExactlyAtRetryLimit_ThrowsException()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 3, RetryDelayMilliseconds = 10 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        MessageHandlerDelegate next = () =>
        {
            executionCount++;
            throw new InvalidOperationException($"Attempt {executionCount}");
        };

        // Act & Assert
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => behavior.HandleAsync(message, next, CancellationToken.None));
        Assert.AreEqual(3, executionCount);
        Assert.AreEqual("Attempt 3", exception.Message);
    }

    [TestMethod]
    public async Task HandleAsync_DifferentExceptionTypes_RetriesForAll()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 4, RetryDelayMilliseconds = 10 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        MessageHandlerDelegate next = () =>
        {
            executionCount++;
            throw executionCount switch
            {
                1 => new InvalidOperationException("First"),
                2 => new ArgumentException("Second"),
                3 => new NullReferenceException("Third"),
                _ => new Exception("Fourth")
            };
        };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            () => behavior.HandleAsync(message, next, CancellationToken.None));
        Assert.AreEqual(4, executionCount);
    }

    [TestMethod]
    public async Task HandleAsync_ZeroDelayMilliseconds_RetriesWithoutDelay()
    {
        // Arrange
        var options = new FluxorBusOptions { RetryAttempts = 3, RetryDelayMilliseconds = 0 };
        var behavior = new RetryBehavior<TestMessage>(options);
        var message = new TestMessage();
        var executionCount = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await behavior.HandleAsync(message, Next, CancellationToken.None);

        // Assert
        stopwatch.Stop();
        Assert.AreEqual(3, executionCount);
        Assert.IsLessThan(50, stopwatch.ElapsedMilliseconds, $"Should complete quickly with no delay, took {stopwatch.ElapsedMilliseconds}ms");
        return;

        Task Next()
        {
            executionCount++;
            return executionCount < 3 ? throw new InvalidOperationException($"Attempt {executionCount} fails") : Task.CompletedTask;
        }
    }
}
