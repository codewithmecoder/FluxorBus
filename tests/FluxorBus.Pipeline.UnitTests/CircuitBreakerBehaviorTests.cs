using FluxorBus.Abstractions;
using FluxorBus.Core;

namespace FluxorBus.Pipeline.UnitTests;

[TestClass]
public class CircuitBreakerBehaviorTests
{
    private sealed record TestMessage : IMessage;

    [TestMethod]
    public async Task HandleAsync_SuccessfulExecution_ResetsFailureCount()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 3 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        var nextCalled = false;

        // Act
        await behavior.HandleAsync(message, Next, CancellationToken.None);

        // Assert
        Assert.IsTrue(nextCalled);
        return;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_CircuitOpen_ThrowsException()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 1 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        MessageHandlerDelegate failingNext = () => throw new InvalidOperationException("Test failure");

        // Act - cause failures to open circuit
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));

        // Assert - circuit should now be open
        var exception = await Assert.ThrowsExactlyAsync<Exception>(
            async () => await behavior.HandleAsync(message, () => Task.CompletedTask, CancellationToken.None));
        Assert.AreEqual("Circuit Open", exception.Message);
    }

    [TestMethod]
    public async Task HandleAsync_SingleFailureBelowThreshold_DoesNotOpenCircuit()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 3 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        MessageHandlerDelegate failingNext = () => throw new InvalidOperationException("Test failure");

        // Act & Assert - first failure
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));

        // Circuit should still be closed - successful call should work
        await behavior.HandleAsync(message, SuccessNext, CancellationToken.None);
        return;

        Task SuccessNext() => Task.CompletedTask;
    }

    [TestMethod]
    public async Task HandleAsync_FailuresAtThreshold_DoesNotOpenCircuit()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 3 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        MessageHandlerDelegate failingNext = () => throw new InvalidOperationException("Test failure");

        // Act & Assert - exactly at threshold (3 failures)
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));

        // Circuit should still be closed at exactly the threshold
        await behavior.HandleAsync(message, SuccessNext, CancellationToken.None);
        return;

        Task SuccessNext() => Task.CompletedTask;
    }

    [TestMethod]
    public async Task HandleAsync_FailuresExceedThreshold_OpensCircuit()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 2 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        MessageHandlerDelegate failingNext = () => throw new InvalidOperationException("Test failure");

        // Act - fail 3 times (exceeding threshold of 2)
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));

        // Assert - circuit should now be open
        var exception = await Assert.ThrowsExactlyAsync<Exception>(
            async () => await behavior.HandleAsync(message, () => Task.CompletedTask, CancellationToken.None));
        Assert.AreEqual("Circuit Open", exception.Message);
    }

    [TestMethod]
    public async Task HandleAsync_SuccessAfterFailure_ResetsFailureCount()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 3 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        MessageHandlerDelegate failingNext = () => throw new InvalidOperationException("Test failure");
        MessageHandlerDelegate successNext = () => Task.CompletedTask;

        // Act - fail twice, then succeed
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await behavior.HandleAsync(message, successNext, CancellationToken.None);

        // Now we can fail 3 more times before circuit opens (proving reset)
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));

        // Circuit should still be closed after exactly 3 failures
        await behavior.HandleAsync(message, successNext, CancellationToken.None);
    }

    [TestMethod]
    public async Task HandleAsync_ExceptionPropagated_RethrowsOriginalException()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 5 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        var expectedException = new InvalidOperationException("Test failure");
        MessageHandlerDelegate failingNext = () => throw expectedException;

        // Act & Assert
        var actualException = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));
        Assert.AreEqual(expectedException.Message, actualException.Message);
    }

    [TestMethod]
    public async Task HandleAsync_NextDelegateInvoked_CallsDelegate()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 5 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        var delegateCalled = false;

        // Act
        await behavior.HandleAsync(message, Next, CancellationToken.None);

        // Assert
        Assert.IsTrue(delegateCalled);
        return;

        Task Next()
        {
            delegateCalled = true;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task HandleAsync_ThresholdOfZero_OpensOnFirstFailure()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 0 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        MessageHandlerDelegate failingNext = () => throw new InvalidOperationException("Test failure");

        // Act - first failure should open circuit (threshold is 0)
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, failingNext, CancellationToken.None));

        // Assert - circuit should now be open
        var exception = await Assert.ThrowsExactlyAsync<Exception>(
            async () => await behavior.HandleAsync(message, () => Task.CompletedTask, CancellationToken.None));
        Assert.AreEqual("Circuit Open", exception.Message);
    }

    [TestMethod]
    public async Task HandleAsync_MultipleSuccessfulCalls_MaintainsClosedCircuit()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 3 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();
        var callCount = 0;
        MessageHandlerDelegate successNext = () =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        // Act
        await behavior.HandleAsync(message, successNext, CancellationToken.None);
        await behavior.HandleAsync(message, successNext, CancellationToken.None);
        await behavior.HandleAsync(message, successNext, CancellationToken.None);

        // Assert
        Assert.AreEqual(3, callCount);
    }

    [TestMethod]
    public async Task HandleAsync_DifferentExceptionTypes_AllCountedAsFailures()
    {
        // Arrange
        var options = new FluxorBusOptions { CircuitBreakerFailureThreshold = 2 };
        var behavior = new CircuitBreakerBehavior<TestMessage>(options);
        var message = new TestMessage();

        // Act - different exception types
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await behavior.HandleAsync(message, () => throw new InvalidOperationException(), CancellationToken.None));
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await behavior.HandleAsync(message, () => throw new ArgumentException(), CancellationToken.None));
        await Assert.ThrowsExactlyAsync<NullReferenceException>(
            async () => await behavior.HandleAsync(message, () => throw new NullReferenceException(), CancellationToken.None));

        // Assert - circuit should now be open
        var exception = await Assert.ThrowsExactlyAsync<Exception>(
            async () => await behavior.HandleAsync(message, () => Task.CompletedTask, CancellationToken.None));
        Assert.AreEqual("Circuit Open", exception.Message);
    }
}
