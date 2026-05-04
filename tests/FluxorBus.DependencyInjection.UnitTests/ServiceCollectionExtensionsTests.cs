using FluxorBus.Abstractions;
using FluxorBus.Core;
using FluxorBus.Core.Channel;
using FluxorBus.Core.Execution;
using FluxorBus.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluxorBus.DependencyInjection.UnitTests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddFluxorBus_WithNullConfigure_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddFluxorBus();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddFluxorBus_WithNullConfigure_RegistersFluxorBusOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<FluxorBusOptions>();
        Assert.IsNotNull(options);
    }

    [TestMethod]
    public void AddFluxorBus_WithConfigure_InvokesConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configureInvoked = false;

        // Act
        services.AddFluxorBus(_ =>
        {
            configureInvoked = true;
        });

        // Assert
        Assert.IsTrue(configureInvoked);
    }

    [TestMethod]
    public void AddFluxorBus_WithConfigure_PassesOptionsToConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        FluxorBusOptions? capturedOptions = null;

        // Act
        services.AddFluxorBus(options =>
        {
            capturedOptions = options;
        });

        // Assert
        Assert.IsNotNull(capturedOptions);
    }

    [TestMethod]
    public void AddFluxorBus_WithConfigure_RegistersConfiguredOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        const int expectedCapacity = 500;

        // Act
        services.AddFluxorBus(options =>
        {
            options.Capacity = expectedCapacity;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var registeredOptions = serviceProvider.GetRequiredService<FluxorBusOptions>();
        Assert.AreEqual(expectedCapacity, registeredOptions.Capacity);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersChannelMessageBusAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ChannelMessageBus));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersIMessageBusAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMessageBus));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_IMessageBus_ResolvesToChannelMessageBus()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFluxorBus();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
        var channelMessageBus = serviceProvider.GetRequiredService<ChannelMessageBus>();

        // Assert
        Assert.AreSame(channelMessageBus, messageBus);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersMessageConsumerAsHostedService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(MessageConsumer));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersMessageExecutorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(MessageExecutor<>));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersMessageBatchExecutorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(MessageBatchExecutor<>));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersRetryBehaviorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<>) &&
            s.ImplementationType?.GetGenericTypeDefinition() == typeof(RetryBehavior<>));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersCircuitBreakerBehaviorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<>) &&
            s.ImplementationType?.GetGenericTypeDefinition() == typeof(CircuitBreakerBehavior<>));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersMetricsBehaviorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<>) &&
            s.ImplementationType?.GetGenericTypeDefinition() == typeof(MetricsBehavior<>));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddFluxorBus_RegistersAllThreePipelineBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFluxorBus();

        // Assert
        var pipelineBehaviors = services.Where(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<>)).ToList();
        Assert.HasCount(3, pipelineBehaviors);
    }

    [TestMethod]
    public void AddFluxorBus_AllRegisteredServices_CanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFluxorBus();

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetRequiredService<FluxorBusOptions>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<ChannelMessageBus>());
        Assert.IsNotNull(serviceProvider.GetRequiredService<IMessageBus>());
        Assert.IsNotNull(serviceProvider.GetServices<IHostedService>().FirstOrDefault(s => s is MessageConsumer));
    }
}
