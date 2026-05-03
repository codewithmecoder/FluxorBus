using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace FluxorBus.Core.Execution;

/// <summary>
/// Provides a factory for retrieving or creating message executors for specific message types using dependency
/// injection.
/// </summary>
/// <remarks>This factory caches created executors to improve performance when retrieving executors for the same
/// message type multiple times. The factory relies on the dependency injection container to resolve executor instances,
/// so all required executor types must be registered with the service provider.</remarks>
/// <param name="sp">The service provider used to resolve message executor instances.</param>
public class MessageExecutorFactory(IServiceProvider sp)
{
    private readonly ConcurrentDictionary<Type, IMessageExecutor> _cache = new();

    /// <summary>
    /// Retrieves an executor instance for the specified message type, creating and caching it if necessary.
    /// </summary>
    /// <remarks>Subsequent calls with the same type return the cached executor instance. This method is
    /// thread-safe.</remarks>
    /// <param name="type">The type of message for which to retrieve the executor. Cannot be null.</param>
    /// <returns>An executor instance that can process messages of the specified type.</returns>
    public IMessageExecutor Get(Type type)
    {
        return _cache.GetOrAdd(type, Create);
    }

    private IMessageExecutor Create(Type type)
    {
        var executorType = typeof(MessageExecutor<>).MakeGenericType(type);
        return (IMessageExecutor)sp.GetRequiredService(executorType);
    }
} 