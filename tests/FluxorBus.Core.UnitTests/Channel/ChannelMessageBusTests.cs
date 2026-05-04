using FluxorBus.Abstractions;
using FluxorBus.Core.Channel;

namespace FluxorBus.Core.UnitTests.Channel;

[TestClass]
public class ChannelMessageBusTests
{
    private sealed record TestMessage : IMessage;
    private sealed record AnotherTestMessage(string Data) : IMessage;

    [TestMethod]
    public async Task PublishAsync_ValidMessage_WritesToChannel()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message = new TestMessage();

        // Act
        await bus.PublishAsync(message, TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        var reader = bus.GetReader();
        var readMessage = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.AreEqual(message, readMessage);
    }

    [TestMethod]
    public async Task PublishAsync_WithCancellationToken_PublishesMessage()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message = new TestMessage();
        using var cts = new CancellationTokenSource();

        // Act
        await bus.PublishAsync(message, cts.Token);

        // Assert
        var reader = bus.GetReader();
        var readMessage = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.AreEqual(message, readMessage);
    }

    [TestMethod]
    public async Task PublishAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message = new TestMessage();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        try
        {
            await bus.PublishAsync(message, cts.Token);
            Assert.Fail("Expected OperationCanceledException was not thrown.");
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task PublishAsync_MultipleMessages_AllMessagesPublished()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message1 = new TestMessage();
        var message2 = new TestMessage();
        var message3 = new TestMessage();

        // Act
        await bus.PublishAsync(message1, TestContext?.CancellationToken ?? CancellationToken.None);
        await bus.PublishAsync(message2, TestContext?.CancellationToken ?? CancellationToken.None);
        await bus.PublishAsync(message3, TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        var reader = bus.GetReader();
        var readMessage1 = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        var readMessage2 = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        var readMessage3 = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.AreEqual(message1, readMessage1);
        Assert.AreEqual(message2, readMessage2);
        Assert.AreEqual(message3, readMessage3);
    }

    [TestMethod]
    public async Task ReadAllAsync_WithMessages_ReturnsAllMessages()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message1 = new TestMessage();
        var message2 = new TestMessage();
        await bus.PublishAsync(message1);
        await bus.PublishAsync(message2);

        // Act
        var messages = new List<IMessage>();
        var readTask = Task.Run(async () =>
        {
            await foreach (var message in bus.ReadAllAsync(CancellationToken.None))
            {
                messages.Add(message);
                if (messages.Count == 2)
                    break;
            }
        });

        await readTask;

        // Assert
        Assert.HasCount(2, messages);
        Assert.AreEqual(message1, messages[0]);
        Assert.AreEqual(message2, messages[1]);
    }

    [TestMethod]
    public async Task ReadAllAsync_WithCancellationToken_StopsReading()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message1 = new TestMessage();
        await bus.PublishAsync(message1, TestContext?.CancellationToken ?? CancellationToken.None);
        using var cts = new CancellationTokenSource();

        // Act
        var messages = new List<IMessage>();
        await cts.CancelAsync();

        // Assert
        try
        {
            await foreach (var message in bus.ReadAllAsync(cts.Token))
            {
                messages.Add(message);
            }
            Assert.Fail("Expected OperationCanceledException was not thrown.");
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task ReadAllAsync_EmptyCompletedChannel_ReturnsEmpty()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);

        // Act
        var messages = new List<IMessage>();
        using var cts = new CancellationTokenSource(100);
        try
        {
            await foreach (var message in bus.ReadAllAsync(cts.Token))
            {
                messages.Add(message);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when no messages and we timeout
        }

        // Assert
        Assert.IsEmpty(messages);
    }

    [TestMethod]
    public void GetReader_ReturnsNonNull()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);

        // Act
        var reader = bus.GetReader();

        // Assert
        Assert.IsNotNull(reader);
    }

    [TestMethod]
    public void GetReader_CalledMultipleTimes_ReturnsSameInstance()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);

        // Act
        var reader1 = bus.GetReader();
        var reader2 = bus.GetReader();

        // Assert
        Assert.AreSame(reader1, reader2);
    }

    [TestMethod]
    public async Task GetReader_CanReadPublishedMessages()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message = new TestMessage();
        await bus.PublishAsync(message);

        // Act
        var reader = bus.GetReader();
        var canRead = await reader.WaitToReadAsync();
        var success = reader.TryRead(out var readMessage);

        // Assert
        Assert.IsTrue(canRead);
        Assert.IsTrue(success);
        Assert.AreEqual(message, readMessage);
    }

    [TestMethod]
    public async Task PublishAsync_WhenChannelFull_WaitsForSpace()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 1 };
        var bus = new ChannelMessageBus(options);
        var message1 = new TestMessage();
        var message2 = new TestMessage();
        await bus.PublishAsync(message1, TestContext?.CancellationToken ?? CancellationToken.None);

        // Act
        var publishTask = bus.PublishAsync(message2, TestContext?.CancellationToken ?? CancellationToken.None).AsTask();
        await Task.Delay(50, TestContext?.CancellationToken ?? CancellationToken.None); // Give it time to start waiting
        Assert.IsFalse(publishTask.IsCompleted);

        // Read first message to make space
        var reader = bus.GetReader();
        await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);

        // Now the second publish should complete
        await publishTask;

        // Assert
        var readMessage2 = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.AreEqual(message2, readMessage2);
    }

    [TestMethod]
    public async Task PublishAsync_CanceledWhileWaitingForSpace_ThrowsOperationCanceledException()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 1 };
        var bus = new ChannelMessageBus(options);
        var message1 = new TestMessage();
        var message2 = new TestMessage();
        await bus.PublishAsync(message1, TestContext?.CancellationToken ?? CancellationToken.None);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        var publishTask = bus.PublishAsync(message2, cts.Token).AsTask();
        await Task.Delay(50, TestContext?.CancellationToken ?? CancellationToken.None); // Give it time to start waiting
        await cts.CancelAsync();

        try
        {
            await publishTask;
            Assert.Fail("Expected OperationCanceledException was not thrown.");
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task PublishAsync_DifferentMessageType_WritesToChannel()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message = new AnotherTestMessage("test data");

        // Act
        await bus.PublishAsync(message, TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        var reader = bus.GetReader();
        var readMessage = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.AreEqual(message, readMessage);
    }

    [TestMethod]
    public async Task PublishAsync_MessageWithState_PreservesMessageState()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        const string expectedData = "important data";
        var message = new AnotherTestMessage(expectedData);

        // Act
        await bus.PublishAsync(message, TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        var reader = bus.GetReader();
        var readMessage = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.IsInstanceOfType<AnotherTestMessage>(readMessage);
        var typedMessage = (AnotherTestMessage)readMessage;
        Assert.AreEqual(expectedData, typedMessage.Data);
    }

    [TestMethod]
    public async Task PublishAsync_ConcurrentPublishes_AllMessagesPublished()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 100 };
        var bus = new ChannelMessageBus(options);
        const int messageCount = 10;
        var messages = Enumerable.Range(0, messageCount)
            .Select(i => new AnotherTestMessage($"Message {i}"))
            .ToList();

        // Act
        var publishTasks = messages.Select(m => bus.PublishAsync(m, TestContext?.CancellationToken ?? CancellationToken.None).AsTask()).ToList();
        await Task.WhenAll(publishTasks);

        // Assert
        var reader = bus.GetReader();
        var readMessages = new List<IMessage>();
        for (var i = 0; i < messageCount; i++)
        {
            readMessages.Add(await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None));
        }

        Assert.HasCount(messageCount, readMessages);
        foreach (var message in messages)
        {
            Assert.Contains(message, readMessages);
        }
    }

    [TestMethod]
    public async Task PublishAsync_SameMessageInstanceMultipleTimes_PublishesAllInstances()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message = new TestMessage();

        // Act
        await bus.PublishAsync(message, TestContext?.CancellationToken ?? CancellationToken.None);
        await bus.PublishAsync(message, TestContext?.CancellationToken ?? CancellationToken.None);
        await bus.PublishAsync(message, TestContext?.CancellationToken ?? CancellationToken.None);

        // Assert
        var reader = bus.GetReader();
        var readMessage1 = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        var readMessage2 = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        var readMessage3 = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.AreSame(message, readMessage1);
        Assert.AreSame(message, readMessage2);
        Assert.AreSame(message, readMessage3);
    }

    [TestMethod]
    public async Task PublishAsync_WithoutCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var options = new FluxorBusOptions { Capacity = 10 };
        var bus = new ChannelMessageBus(options);
        var message = new TestMessage();

        // Act
        var publishTask = bus.PublishAsync(message, TestContext?.CancellationToken ?? CancellationToken.None);
        await publishTask;

        // Assert
        var reader = bus.GetReader();
        var readMessage = await reader.ReadAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.AreEqual(message, readMessage);
        Assert.IsTrue(publishTask.IsCompleted);
    }
    
    public TestContext? TestContext { get; set; }
}
