using FluxorBus.Abstractions;
using FluxorBus.SampleApi.Features.Orders;
using Microsoft.AspNetCore.Mvc;

namespace FluxorBus.SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController(IMessageBus bus) : ControllerBase
{
    [HttpPost("single-order")]
    public async Task<IActionResult> CreateOrder()
    {
        var order = new OrderCreated(Guid.NewGuid(), Random.Shared.Next(10, 500));

        await bus.PublishAsync(order);

        return Ok(new { order.OrderId });
    }
    [HttpPost("single-order-process-as-batch")]
    public async Task<IActionResult> CreateOrderBatch()
    {
        var order = new OrderCreatedBatch(Guid.NewGuid(), Random.Shared.Next(10, 500));

        await bus.PublishAsync(order);

        return Ok(new { order.OrderId });
    }
}