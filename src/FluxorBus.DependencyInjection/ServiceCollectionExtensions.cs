using FluxorBus.Abstractions;
using FluxorBus.Core.Channel;
using FluxorBus.Core.Execution;
using FluxorBus.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace FluxorBus.DependencyInjection;

/// <summary>
/// Provides extension methods for registering Fluxor bus messaging services and related pipeline behaviors with an
/// IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Fluxor bus messaging services and related pipeline behaviors to the specified service collection.
    /// </summary>
    /// <remarks>This method registers the message bus, message executor, handler registry, and several
    /// pipeline behaviors required for Fluxor-based messaging. It should be called during application startup to enable
    /// message-based communication and middleware behaviors such as retries, circuit breaking, and metrics.</remarks>
    /// <param name="services">The service collection to which the Fluxor bus and related services are added. Cannot be null.</param>
    /// <returns>The same service collection instance, enabling method chaining.</returns>
    public static IServiceCollection AddFluxorBus(this IServiceCollection services)
    {
        services.AddSingleton<ChannelMessageBus>();
        services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<ChannelMessageBus>());

        services.AddSingleton<MessageExecutorFactory>();
        services.AddSingleton<MessageHandlerRegistryFactory>();

        services.AddHostedService<MessageConsumer>();

        services.AddTransient(typeof(MessageExecutor<>));

        services.AddScoped(typeof(IPipelineBehavior<>), typeof(RetryBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(CircuitBreakerBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(MetricsBehavior<>));

        return services;
    }
}