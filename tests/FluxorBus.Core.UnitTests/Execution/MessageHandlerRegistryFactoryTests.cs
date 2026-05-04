using FluxorBus.Abstractions;
using FluxorBus.Core.Execution;
using Moq;

namespace FluxorBus.Core.UnitTests.Execution;

[TestClass]
public class MessageHandlerRegistryFactoryTests
{
    [TestMethod]
    public void Create_WithNoHandlersAndNoBehaviors_ReturnsRegistryWithEmptyArrays()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithHandlersButNoBehaviors_ReturnsRegistryWithHandlers()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handler1Mock = new Mock<IMessageHandler<TestMessage>>();
        var handler2Mock = new Mock<IMessageHandler<TestMessage>>();
        var handlers = new[] { handler1Mock.Object, handler2Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithBehaviorsButNoHandlers_ReturnsRegistryWithBehaviors()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var behavior1Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behavior2Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behaviors = new[] { behavior1Mock.Object, behavior2Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviors);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithHandlersAndBehaviors_ReturnsRegistryWithBoth()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handler1Mock = new Mock<IMessageHandler<TestMessage>>();
        var handler2Mock = new Mock<IMessageHandler<TestMessage>>();
        var behavior1Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behavior2Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var handlers = new[] { handler1Mock.Object, handler2Mock.Object };
        var behaviors = new[] { behavior1Mock.Object, behavior2Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviors);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithSingleHandler_ReturnsRegistryWithOneHandler()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handlerMock = new Mock<IMessageHandler<TestMessage>>();
        var handlers = new[] { handlerMock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithSingleBehavior_ReturnsRegistryWithOneBehavior()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var behaviorMock = new Mock<IPipelineBehavior<TestMessage>>();
        var behaviors = new[] { behaviorMock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviors);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithMultipleHandlers_ReturnsRegistryWithAllHandlers()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handler1Mock = new Mock<IMessageHandler<TestMessage>>();
        var handler2Mock = new Mock<IMessageHandler<TestMessage>>();
        var handler3Mock = new Mock<IMessageHandler<TestMessage>>();
        var handlers = new[] { handler1Mock.Object, handler2Mock.Object, handler3Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithMultipleBehaviors_ReturnsRegistryWithAllBehaviors()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var behavior1Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behavior2Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behavior3Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behaviors = new[] { behavior1Mock.Object, behavior2Mock.Object, behavior3Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviors);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_WithDifferentMessageType_ReturnsRegistryForThatType()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handlerMock = new Mock<IMessageHandler<AnotherTestMessage>>();
        var handlers = new[] { handlerMock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<AnotherTestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<AnotherTestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<AnotherTestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<AnotherTestMessage>();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Create_CallsGetServicesForHandlers_VerifiesServiceProviderInteraction()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        _ = factory.Create<TestMessage>();

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)), Times.Once);
    }

    [TestMethod]
    public void Create_CallsGetServicesForBehaviors_VerifiesServiceProviderInteraction()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        _ = factory.Create<TestMessage>();

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)), Times.Once);
    }

    [TestMethod]
    public void Create_WithHandlers_RegistryContainsCorrectHandlers()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handler1Mock = new Mock<IMessageHandler<TestMessage>>();
        var handler2Mock = new Mock<IMessageHandler<TestMessage>>();
        var handlers = new[] { handler1Mock.Object, handler2Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.HasCount(2, result.Handlers);
        Assert.AreSame(handler1Mock.Object, result.Handlers[0]);
        Assert.AreSame(handler2Mock.Object, result.Handlers[1]);
    }

    [TestMethod]
    public void Create_WithBehaviors_RegistryContainsCorrectBehaviors()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var behavior1Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behavior2Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behaviors = new[] { behavior1Mock.Object, behavior2Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviors);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.HasCount(2, result.Behaviors);
        Assert.AreSame(behavior1Mock.Object, result.Behaviors[0]);
        Assert.AreSame(behavior2Mock.Object, result.Behaviors[1]);
    }

    [TestMethod]
    public void Create_WithEmptyHandlersAndBehaviors_RegistryContainsEmptyArrays()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsEmpty(result.Handlers);
        Assert.IsEmpty(result.Behaviors);
    }

    [TestMethod]
    public void Create_PreservesHandlerOrder()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handler1Mock = new Mock<IMessageHandler<TestMessage>>();
        var handler2Mock = new Mock<IMessageHandler<TestMessage>>();
        var handler3Mock = new Mock<IMessageHandler<TestMessage>>();
        var handlers = new[] { handler1Mock.Object, handler2Mock.Object, handler3Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.HasCount(3, result.Handlers);
        Assert.AreSame(handler1Mock.Object, result.Handlers[0]);
        Assert.AreSame(handler2Mock.Object, result.Handlers[1]);
        Assert.AreSame(handler3Mock.Object, result.Handlers[2]);
    }

    [TestMethod]
    public void Create_PreservesBehaviorOrder()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var behavior1Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behavior2Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behavior3Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behaviors = new[] { behavior1Mock.Object, behavior2Mock.Object, behavior3Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviors);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.HasCount(3, result.Behaviors);
        Assert.AreSame(behavior1Mock.Object, result.Behaviors[0]);
        Assert.AreSame(behavior2Mock.Object, result.Behaviors[1]);
        Assert.AreSame(behavior3Mock.Object, result.Behaviors[2]);
    }

    [TestMethod]
    public void Create_ConvertsHandlersEnumerableToArray()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handler1Mock = new Mock<IMessageHandler<TestMessage>>();
        var handlersList = new List<IMessageHandler<TestMessage>> { handler1Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlersList);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsInstanceOfType<IMessageHandler<TestMessage>[]>(result.Handlers);
        Assert.HasCount(1, result.Handlers);
    }

    [TestMethod]
    public void Create_ConvertsBehaviorsEnumerableToArray()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var behavior1Mock = new Mock<IPipelineBehavior<TestMessage>>();
        var behaviorsList = new List<IPipelineBehavior<TestMessage>> { behavior1Mock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviorsList);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Create<TestMessage>();

        // Assert
        Assert.IsInstanceOfType<IPipelineBehavior<TestMessage>[]>(result.Behaviors);
        Assert.HasCount(1, result.Behaviors);
    }

    [TestMethod]
    public void Create_CalledMultipleTimes_ReturnsIndependentRegistries()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handlerMock = new Mock<IMessageHandler<TestMessage>>();
        var handlers = new[] { handlerMock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result1 = factory.Create<TestMessage>();
        var result2 = factory.Create<TestMessage>();

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreNotSame(result1, result2);
    }

    [TestMethod]
    public void Create_WithDifferentMessageTypes_CreatesCorrectRegistriesForEachType()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var testMessageHandlerMock = new Mock<IMessageHandler<TestMessage>>();
        var anotherMessageHandlerMock = new Mock<IMessageHandler<AnotherTestMessage>>();
        var testMessageHandlers = new[] { testMessageHandlerMock.Object };
        var anotherMessageHandlers = new[] { anotherMessageHandlerMock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(testMessageHandlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<AnotherTestMessage>>)))
            .Returns(anotherMessageHandlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<AnotherTestMessage>>)))
            .Returns(Enumerable.Empty<IPipelineBehavior<AnotherTestMessage>>());

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var testResult = factory.Create<TestMessage>();
        var anotherResult = factory.Create<AnotherTestMessage>();

        // Assert
        Assert.IsNotNull(testResult);
        Assert.IsNotNull(anotherResult);
        Assert.HasCount(1, testResult.Handlers);
        Assert.HasCount(1, anotherResult.Handlers);
        Assert.AreSame(testMessageHandlerMock.Object, testResult.Handlers[0]);
        Assert.AreSame(anotherMessageHandlerMock.Object, anotherResult.Handlers[0]);
    }

    [TestMethod]
    public void Create_WhenServiceProviderThrowsForHandlers_ExceptionPropagates()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Throws(new InvalidOperationException("Service provider error"));

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => factory.Create<TestMessage>());
    }

    [TestMethod]
    public void Create_WhenServiceProviderThrowsForBehaviors_ExceptionPropagates()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(Enumerable.Empty<IMessageHandler<TestMessage>>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Throws(new InvalidOperationException("Service provider error"));

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => factory.Create<TestMessage>());
    }

    [TestMethod]
    public void Create_ReturnsNewArrayInstances_NotCached()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var handlerMock = new Mock<IMessageHandler<TestMessage>>();
        var behaviorMock = new Mock<IPipelineBehavior<TestMessage>>();
        var handlers = new[] { handlerMock.Object };
        var behaviors = new[] { behaviorMock.Object };

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IMessageHandler<TestMessage>>)))
            .Returns(handlers);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestMessage>>)))
            .Returns(behaviors);

        var factory = new MessageHandlerRegistryFactory(serviceProviderMock.Object);

        // Act
        var result1 = factory.Create<TestMessage>();
        var result2 = factory.Create<TestMessage>();

        // Assert
        Assert.AreNotSame(result1.Handlers, result2.Handlers);
        Assert.AreNotSame(result1.Behaviors, result2.Behaviors);
    }

    public class TestMessage : IMessage
    {
    }

    public class AnotherTestMessage : IMessage
    {
    }
}
