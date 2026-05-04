using FluxorBus.Abstractions;
using FluxorBus.Core.Execution;
using Moq;

namespace FluxorBus.Core.UnitTests.Execution;

[TestClass]
public class MessageExecutorFactoryTests
{
    [TestMethod]
    public void Get_WithType_ReturnsExecutor()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var executorMock = new Mock<IMessageExecutor>();
        var messageType = typeof(TestMessage);
        var expectedExecutorType = typeof(MessageExecutor<TestMessage>);

        serviceProviderMock
            .Setup(sp => sp.GetService(expectedExecutorType))
            .Returns(executorMock.Object);

        var factory = new MessageExecutorFactory(serviceProviderMock.Object);

        // Act
        var result = factory.Get(messageType);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(executorMock.Object, result);
    }

    [TestMethod]
    public void Get_WithSameTypeTwice_ReturnsCachedExecutor()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var executorMock = new Mock<IMessageExecutor>();
        var messageType = typeof(TestMessage);
        var expectedExecutorType = typeof(MessageExecutor<TestMessage>);

        serviceProviderMock
            .Setup(sp => sp.GetService(expectedExecutorType))
            .Returns(executorMock.Object);

        var factory = new MessageExecutorFactory(serviceProviderMock.Object);

        // Act
        var result1 = factory.Get(messageType);
        var result2 = factory.Get(messageType);

        // Assert
        Assert.AreSame(result1, result2);
        serviceProviderMock.Verify(sp => sp.GetService(expectedExecutorType), Times.Once);
    }

    [TestMethod]
    public void Get_WithDifferentTypes_ReturnsDifferentExecutors()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var executor1Mock = new Mock<IMessageExecutor>();
        var executor2Mock = new Mock<IMessageExecutor>();
        var messageType1 = typeof(TestMessage);
        var messageType2 = typeof(AnotherTestMessage);
        var executorType1 = typeof(MessageExecutor<TestMessage>);
        var executorType2 = typeof(MessageExecutor<AnotherTestMessage>);

        serviceProviderMock
            .Setup(sp => sp.GetService(executorType1))
            .Returns(executor1Mock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(executorType2))
            .Returns(executor2Mock.Object);

        var factory = new MessageExecutorFactory(serviceProviderMock.Object);

        // Act
        var result1 = factory.Get(messageType1);
        var result2 = factory.Get(messageType2);

        // Assert
        Assert.AreNotSame(result1, result2);
        Assert.AreSame(executor1Mock.Object, result1);
        Assert.AreSame(executor2Mock.Object, result2);
    }

    [TestMethod]
    public void Get_CalledConcurrentlyWithSameType_ReturnsConsistentExecutor()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var executorMock = new Mock<IMessageExecutor>();
        var messageType = typeof(TestMessage);
        var expectedExecutorType = typeof(MessageExecutor<TestMessage>);

        serviceProviderMock
            .Setup(sp => sp.GetService(expectedExecutorType))
            .Returns(executorMock.Object);

        var factory = new MessageExecutorFactory(serviceProviderMock.Object);
        var results = new IMessageExecutor[10];

        // Act
        Parallel.For(0, 10, i =>
        {
            results[i] = factory.Get(messageType);
        });

        // Assert
        Assert.IsTrue(results.All(r => ReferenceEquals(r, results[0])));
    }

    [TestMethod]
    public void Get_WithMultipleTypes_MaintainsSeparateCacheEntries()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var executor1Mock = new Mock<IMessageExecutor>();
        var executor2Mock = new Mock<IMessageExecutor>();
        var messageType1 = typeof(TestMessage);
        var messageType2 = typeof(AnotherTestMessage);
        var executorType1 = typeof(MessageExecutor<TestMessage>);
        var executorType2 = typeof(MessageExecutor<AnotherTestMessage>);

        serviceProviderMock
            .Setup(sp => sp.GetService(executorType1))
            .Returns(executor1Mock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(executorType2))
            .Returns(executor2Mock.Object);

        var factory = new MessageExecutorFactory(serviceProviderMock.Object);

        // Act
        var result1A = factory.Get(messageType1);
        var result2A = factory.Get(messageType2);
        var result1B = factory.Get(messageType1);
        var result2B = factory.Get(messageType2);

        // Assert
        Assert.AreSame(result1A, result1B);
        Assert.AreSame(result2A, result2B);
        Assert.AreNotSame(result1A, result2A);
        serviceProviderMock.Verify(sp => sp.GetService(executorType1), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetService(executorType2), Times.Once);
    }

    private class TestMessage : IMessage;

    private class AnotherTestMessage : IMessage;
}
