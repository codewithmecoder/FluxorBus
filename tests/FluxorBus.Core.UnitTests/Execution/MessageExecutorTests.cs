using FluxorBus.Abstractions;
using FluxorBus.Core.Execution;
using Moq;

namespace FluxorBus.Core.UnitTests.Execution;

[TestClass]
public class MessageExecutorTests
{
    public class TestMessage : IMessage;

    [TestMethod]
    public async Task Execute_NoHandlersNoBehaviors_CompletesSuccessfully()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Array.Empty<IMessageHandler<TestMessage>>());
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Array.Empty<IPipelineBehavior<TestMessage>>());

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        // No exception means success
    }

    [TestMethod]
    public async Task Execute_SingleHandlerNoBehaviors_CallsHandler()
    {
        // Arrange
        var mockHandler = new Mock<IMessageHandler<TestMessage>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Array.Empty<IPipelineBehavior<TestMessage>>());

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(message, ct), Times.Once);
    }

    [TestMethod]
    public async Task Execute_MultipleHandlersNoBehaviors_CallsAllHandlers()
    {
        // Arrange
        var mockHandler1 = new Mock<IMessageHandler<TestMessage>>();
        mockHandler1
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockHandler2 = new Mock<IMessageHandler<TestMessage>>();
        mockHandler2
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockHandler3 = new Mock<IMessageHandler<TestMessage>>();
        mockHandler3
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler1.Object, mockHandler2.Object, mockHandler3.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Array.Empty<IPipelineBehavior<TestMessage>>());

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        mockHandler1.Verify(h => h.HandleAsync(message, ct), Times.Once);
        mockHandler2.Verify(h => h.HandleAsync(message, ct), Times.Once);
        mockHandler3.Verify(h => h.HandleAsync(message, ct), Times.Once);
    }

    [TestMethod]
    public async Task Execute_SingleHandlerSingleBehavior_CallsBehaviorThenHandler()
    {
        // Arrange
        var callOrder = new List<string>();

        var mockHandler = new Mock<IMessageHandler<TestMessage>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callOrder.Add("Handler");
                return Task.CompletedTask;
            });

        var mockBehavior = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage _, MessageHandlerDelegate next, CancellationToken _) =>
            {
                callOrder.Add("Behavior");
                await next();
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(new[] { mockBehavior.Object });

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        Assert.HasCount(2, callOrder);
        Assert.AreEqual("Behavior", callOrder[0]);
        Assert.AreEqual("Handler", callOrder[1]);
    }

    [TestMethod]
    public async Task Execute_SingleHandlerMultipleBehaviors_CallsBehaviorsInOrder()
    {
        // Arrange
        var callOrder = new List<string>();

        var mockHandler = new Mock<IMessageHandler<TestMessage>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callOrder.Add("Handler");
                return Task.CompletedTask;
            });

        var mockBehavior1 = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior1
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage _, MessageHandlerDelegate next, CancellationToken _) =>
            {
                callOrder.Add("Behavior1");
                await next();
            });

        var mockBehavior2 = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior2
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage _, MessageHandlerDelegate next, CancellationToken _) =>
            {
                callOrder.Add("Behavior2");
                await next();
            });

        var mockBehavior3 = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior3
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage _, MessageHandlerDelegate next, CancellationToken _) =>
            {
                callOrder.Add("Behavior3");
                await next();
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(new[] { mockBehavior1.Object, mockBehavior2.Object, mockBehavior3.Object });

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        Assert.HasCount(4, callOrder);
        Assert.AreEqual("Behavior1", callOrder[0]);
        Assert.AreEqual("Behavior2", callOrder[1]);
        Assert.AreEqual("Behavior3", callOrder[2]);
        Assert.AreEqual("Handler", callOrder[3]);
    }

    [TestMethod]
    public async Task Execute_MultipleHandlersMultipleBehaviors_EachHandlerGetsOwnPipeline()
    {
        // Arrange
        var callOrder = new List<string>();

        var mockHandler1 = new Mock<IMessageHandler<TestMessage>>();
        mockHandler1
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callOrder.Add("Handler1");
                return Task.CompletedTask;
            });

        var mockHandler2 = new Mock<IMessageHandler<TestMessage>>();
        mockHandler2
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callOrder.Add("Handler2");
                return Task.CompletedTask;
            });

        var mockBehavior1 = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior1
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage _, MessageHandlerDelegate next, CancellationToken _) =>
            {
                callOrder.Add("Behavior1");
                await next();
            });

        var mockBehavior2 = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior2
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage _, MessageHandlerDelegate next, CancellationToken _) =>
            {
                callOrder.Add("Behavior2");
                await next();
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler1.Object, mockHandler2.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(new[] { mockBehavior1.Object, mockBehavior2.Object });

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        Assert.HasCount(6, callOrder);
        Assert.AreEqual("Behavior1", callOrder[0]);
        Assert.AreEqual("Behavior2", callOrder[1]);
        Assert.AreEqual("Handler1", callOrder[2]);
        Assert.AreEqual("Behavior1", callOrder[3]);
        Assert.AreEqual("Behavior2", callOrder[4]);
        Assert.AreEqual("Handler2", callOrder[5]);
    }

    [TestMethod]
    public async Task Execute_CancellationTokenPropagated_PassedToHandlerAndBehavior()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var receivedTokens = new List<CancellationToken>();

        var mockHandler = new Mock<IMessageHandler<TestMessage>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns((TestMessage _, CancellationToken ct) =>
            {
                receivedTokens.Add(ct);
                return Task.CompletedTask;
            });

        var mockBehavior = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage _, MessageHandlerDelegate next, CancellationToken ct) =>
            {
                receivedTokens.Add(ct);
                await next();
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(new[] { mockBehavior.Object });

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();

        // Act
        await executor.Execute(message, cts.Token);

        // Assert
        Assert.HasCount(2, receivedTokens);
        Assert.AreEqual(cts.Token, receivedTokens[0]);
        Assert.AreEqual(cts.Token, receivedTokens[1]);
    }

    [TestMethod]
    public async Task Execute_BehaviorDoesNotCallNext_HandlerNotCalled()
    {
        // Arrange
        var mockHandler = new Mock<IMessageHandler<TestMessage>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBehavior = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask); // Does not call next()

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(new[] { mockBehavior.Object });

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Execute_MessageCastCorrectly_HandlerReceivesCorrectMessage()
    {
        // Arrange
        TestMessage? receivedMessage = null;

        var mockHandler = new Mock<IMessageHandler<TestMessage>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns((TestMessage msg, CancellationToken _) =>
            {
                receivedMessage = msg;
                return Task.CompletedTask;
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Array.Empty<IPipelineBehavior<TestMessage>>());

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        Assert.IsNotNull(receivedMessage);
        Assert.AreSame(message, receivedMessage);
    }

    [TestMethod]
    public async Task Execute_BehaviorCanModifyMessage_SubsequentBehaviorsReceiveSameMessage()
    {
        // Arrange
        var receivedMessages = new List<TestMessage>();

        var mockHandler = new Mock<IMessageHandler<TestMessage>>();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<CancellationToken>()))
            .Returns((TestMessage msg, CancellationToken _) =>
            {
                receivedMessages.Add(msg);
                return Task.CompletedTask;
            });

        var mockBehavior1 = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior1
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage msg, MessageHandlerDelegate next, CancellationToken _) =>
            {
                receivedMessages.Add(msg);
                await next();
            });

        var mockBehavior2 = new Mock<IPipelineBehavior<TestMessage>>();
        mockBehavior2
            .Setup(b => b.HandleAsync(It.IsAny<TestMessage>(), It.IsAny<MessageHandlerDelegate>(), It.IsAny<CancellationToken>()))
            .Returns(async (TestMessage msg, MessageHandlerDelegate next, CancellationToken _) =>
            {
                receivedMessages.Add(msg);
                await next();
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(new[] { mockHandler.Object });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(new[] { mockBehavior1.Object, mockBehavior2.Object });

        var executor = new MessageExecutor<TestMessage>(mockServiceProvider.Object);
        var message = new TestMessage();
        var ct = CancellationToken.None;

        // Act
        await executor.Execute(message, ct);

        // Assert
        Assert.HasCount(3, receivedMessages);
        Assert.AreSame(message, receivedMessages[0]);
        Assert.AreSame(message, receivedMessages[1]);
        Assert.AreSame(message, receivedMessages[2]);
    }
}
