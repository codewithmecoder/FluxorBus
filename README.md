# FluxorBus

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**FluxorBus** is a lightweight, high-performance in-process message bus for .NET 10 built on top of `System.Threading.Channels`. It decouples message producers from consumers using a publish/subscribe model, supports multiple handlers per message, and provides a composable pipeline for cross-cutting concerns such as retries, circuit breaking, and metrics.

---

## Table of Contents

- [Purpose](#purpose)
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Registration](#registration)
  - [Define a Message](#define-a-message)
  - [Define a Handler](#define-a-handler)
  - [Publish a Message](#publish-a-message)
  - [Multiple Handlers per Message](#multiple-handlers-per-message)
  - [Batch Messaging](#batch-messaging)
  - [Custom Pipeline Behaviors](#custom-pipeline-behaviors)
  - [Configuration Options](#configuration-options)
- [Built-in Pipeline Behaviors](#built-in-pipeline-behaviors)
- [Source Generator](#source-generator)
- [Sample Project](#sample-project)

---

## Purpose

Modern .NET applications often need to react to domain events without tightly coupling the code that raises the event to the code that handles it. **FluxorBus** solves this by providing:

- A **channel-backed message bus** that queues messages asynchronously and processes them via a background hosted service.
- A **pipeline middleware system** so you can wrap every handler invocation with retry logic, circuit breakers, metrics, logging, or your own custom behaviors — without modifying the handlers themselves.
- A **source generator** (`[MessageHandler]`) that auto-registers handlers at compile time, eliminating reflection-heavy assembly scanning.

---

## Features

| Feature | Description |
|---|---|
| **Channel-based transport** | Uses `System.Threading.Channels` for a lock-free, backpressure-aware in-process queue. |
| **Publish / Subscribe** | Any number of handlers can subscribe to the same message type. |
| **Composable pipeline** | Wrap handler execution with `IPipelineBehavior<T>` middleware (retry, circuit breaker, metrics, etc.). |
| **Batch consuming** | Optionally buffer messages into batches before dispatching to handlers. |
| **Source generator** | `[MessageHandler]` attribute auto-registers handlers — no reflection scanning at startup. |
| **Batch messaging** | Implement `IMessageBatch` + `IMessageBatchHandler<T>` to receive an entire accumulated batch in one handler call — ideal for bulk DB writes. |
| **Hosted service consumer** | A `BackgroundService` drains the channel so publishing is always non-blocking. |
| **First-class DI** | Designed around `Microsoft.Extensions.DependencyInjection` with a single `AddFluxorBus()` call. |
| **.NET 10** | Targets the latest .NET release for maximum performance. |

---

## Architecture

```
Publisher (Controller / Service)
        │
        ▼ PublishAsync<TMessage>
 ┌─────────────────────┐
 │  ChannelMessageBus  │  (bounded channel, capacity configurable)
 └─────────────────────┘
        │
        ▼ background drain (MessageConsumer : BackgroundService)
 ┌─────────────────────┐
 │   MessageExecutor   │  resolves handlers from DI scope
 └─────────────────────┘
        │
        ▼ per handler
 ┌──────────────────────────────────────┐
 │  Pipeline: Retry → CircuitBreaker    │
 │            → Metrics → Handler       │
 └──────────────────────────────────────┘
```

---

## Getting Started

### Installation

```shell
dotnet add package FluxorBus
dotnet add package FluxorBus.SourceGen          # optional compile-time handler registration
```

### Registration

Wire up FluxorBus in `Program.cs`:

```csharp
builder.Services
    .AddFluxorBus(opt =>
    {
        opt.Capacity          = 10_000; // channel buffer size
        opt.EnableBatchConsume = false;
        opt.BatchSize          = 64;    // max messages per batch
        opt.BatchTimeReleased  = 1000;  // ms before a partial batch is flushed
    })
    .AddFluxorBusGenerated(); // registers handlers discovered by the source generator
```

### Define a Message

Implement `IMessage` — a plain record or class works perfectly:

```csharp
using FluxorBus.Abstractions;

public record OrderCreated(Guid OrderId, decimal Amount) : IMessage;
```

### Define a Handler

Implement `IMessageHandler<TMessage>` and decorate the class with `[MessageHandler]` so the source generator registers it automatically:

```csharp
using FluxorBus.Abstractions;
using FluxorBus.SourceGen;

[MessageHandler]
public sealed class OrderCreatedHandler(ILogger<OrderCreatedHandler> logger)
    : IMessageHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated message, CancellationToken ct)
    {
        logger.LogInformation("Saving order {OrderId} — Amount: {Amount}", message.OrderId, message.Amount);
        return Task.CompletedTask;
    }
}
```

### Publish a Message

Inject `IMessageBus` wherever you need to publish:

```csharp
using FluxorBus.Abstractions;

[ApiController]
[Route("[controller]")]
public class OrdersController(IMessageBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var order = new OrderCreated(Guid.NewGuid(), 99.99m);

        await bus.PublishAsync(order); // non-blocking; queued to the channel

        return Ok(new { order.OrderId });
    }
}
```

### Multiple Handlers per Message

You can register as many handlers as you like for the same message — FluxorBus dispatches to all of them:

```csharp
[MessageHandler]
public sealed class SendEmailOnOrderCreated(ILogger<SendEmailOnOrderCreated> logger)
    : IMessageHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated message, CancellationToken ct)
    {
        logger.LogInformation("Sending confirmation email for order {OrderId}", message.OrderId);
        return Task.CompletedTask;
    }
}
```

Both `OrderCreatedHandler` and `SendEmailOnOrderCreated` will be invoked automatically whenever `OrderCreated` is published.

### Batch Messaging

When a message type implements `IMessageBatch` (which extends `IMessage`), FluxorBus accumulates messages of that type and dispatches the **entire list** to every registered `IMessageBatchHandler<T>` in a single call. This is ideal for high-throughput scenarios such as bulk database inserts.

**1. Define a batch message**

```csharp
using FluxorBus.Abstractions;

// Implements IMessageBatch (which extends IMessage) to opt into batch processing
public record ProductViewed(Guid ProductId, string UserId) : IMessageBatch;
```

**2. Enable batch mode and tune the channel**

Batch accumulation only activates when `EnableBatchConsume = true`. Messages are flushed to handlers when either `BatchSize` is reached **or** `BatchTimeReleased` milliseconds elapse — whichever comes first.

```csharp
builder.Services.AddFluxorBus(opt =>
{
    opt.EnableBatchConsume  = true;
    opt.BatchSize           = 64;    // flush after 64 messages
    opt.BatchTimeReleased   = 1000;  // or after 1 second, whichever is first
    opt.Capacity            = 10_000;
});
```

**3. Implement a batch handler**

The handler receives all accumulated messages of that type as a single `IReadOnlyList<T>`:

```csharp
using FluxorBus.Abstractions;

public sealed class ProductViewedBatchHandler(ILogger<ProductViewedBatchHandler> logger)
    : IMessageBatchHandler<ProductViewed>
{
    public Task HandleAsync(IReadOnlyList<ProductViewed> messages, CancellationToken ct)
    {
        logger.LogInformation(
            "Bulk-inserting {Count} product view events into analytics DB",
            messages.Count);

        // e.g. dbContext.ProductViews.AddRange(messages.Select(m => new ProductViewRow(m)));
        // await dbContext.SaveChangesAsync(ct);

        return Task.CompletedTask;
    }
}
```

**4. Register the batch handler**

Batch handlers must be registered manually (the source generator targets `IMessageHandler<T>`, not `IMessageBatchHandler<T>`):

```csharp
builder.Services
    .AddFluxorBus(opt => { opt.EnableBatchConsume = true; /* ... */ })
    .AddFluxorBusGenerated(); // registers regular IMessageHandler<T> handlers

// Register the batch handler explicitly
builder.Services.AddScoped<IMessageBatchHandler<ProductViewed>, ProductViewedBatchHandler>();
```

**5. Publish as usual**

Publishing a batch message is identical to publishing a regular message — FluxorBus detects the `IMessageBatch` marker and routes it automatically:

```csharp
[ApiController]
[Route("[controller]")]
public class ProductsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{productId}")]
    public async Task<IActionResult> ViewProduct(Guid productId, string userId)
    {
        // Fire-and-forget style — returns immediately, no waiting for the handler
        await bus.PublishAsync(new ProductViewed(productId, userId));
        return Ok();
    }
}
```

> **How it works internally**  
> The background `MessageConsumer` separates messages that implement `IMessageBatch` from regular messages. Regular messages are dispatched concurrently via `MessageExecutor<T>`. Batch messages are grouped by type, and each group is passed as a list to `MessageBatchExecutor<T>`, which calls all registered `IMessageBatchHandler<T>` instances sequentially.

---

### Custom Pipeline Behaviors

Implement `IPipelineBehavior<TMessage>` to add cross-cutting logic around every handler invocation:

```csharp
using FluxorBus.Abstractions;

public class LoggingBehavior<T> : IPipelineBehavior<T>
{
    public async Task HandleAsync(T message, MessageHandlerDelegate next, CancellationToken ct)
    {
        Console.WriteLine($"[Before] Handling {typeof(T).Name}");
        await next();
        Console.WriteLine($"[After]  Handled  {typeof(T).Name}");
    }
}
```

Register it in DI:

```csharp
builder.Services.AddScoped(typeof(IPipelineBehavior<>), typeof(LoggingBehavior<>));
```

Behaviors are executed in registration order, wrapping the actual handler.

### Configuration Options

| Property | Default | Description |
|---|---|---|
| `Capacity` | `10000` | Maximum number of messages buffered in the channel. |
| `EnableBatchConsume` | `false` | When `true`, messages are grouped into batches before dispatch. |
| `BatchSize` | `64` | Maximum messages per batch when batch mode is enabled. |
| `BatchTimeReleased` | `1000` | Milliseconds to wait before flushing a partial batch. |

---

## Built-in Pipeline Behaviors

The `FluxorBus.Pipeline` package ships three ready-to-use behaviors, all registered automatically by `AddFluxorBus()`:

### `RetryBehavior<T>`

Retries the handler up to **3 times** on exception, with an increasing delay between attempts.

```
Attempt 1 → failure → wait 50 ms
Attempt 2 → failure → wait 100 ms
Attempt 3 → success (or final exception)
```

### `CircuitBreakerBehavior<T>`

Tracks consecutive failures. After **5 failures** the circuit opens and subsequent calls throw immediately, protecting downstream resources from a cascading failure.

### `MetricsBehavior<T>`

Measures the wall-clock time of each handler invocation and emits a `Debug`-level log entry:

```
[DEBUG] OrderCreated 3ms
```

---

## Source Generator

The `FluxorBus.SourceGen` package provides a Roslyn incremental source generator that scans your assembly at **compile time** for classes decorated with `[MessageHandler]` and generates the DI registration code automatically.

```csharp
// Mark your handler — that's it
[MessageHandler]
public sealed class MyHandler : IMessageHandler<MyMessage> { ... }
```

Then in `Program.cs`:

```csharp
builder.Services
    .AddFluxorBus()
    .AddFluxorBusGenerated(); // calls the generated registration method
```

No runtime assembly scanning. No reflection. Registrations appear as generated C# source in your build output.

---

## Sample Project

The `samples/FluxorBus.SampleApi` project is a minimal ASP.NET Core Web API that demonstrates the full stack:

| File | What it shows |
|---|---|
| `Program.cs` | `AddFluxorBus` + `AddFluxorBusGenerated` wiring |
| `Features/Orders/OrderCreated.cs` | Message definition |
| `Features/Orders/OrderCreatedHandler.cs` | DB-layer handler with `[MessageHandler]` |
| `Features/Orders/SendEmailOnOrderCreated.cs` | Second handler for the same message |
| `Controllers/OrdersController.cs` | Publishing via `IMessageBus` from an API endpoint |

Run the sample:

```shell
cd samples/FluxorBus.SampleApi
dotnet run
```

Then `POST /orders` and watch both handlers fire in the console.
A high-performance, in-memory message bus for .NET built on top of System.Threading.Channels with a powerful middleware pipeline, zero-reflection execution, and source generator based DI registration.
