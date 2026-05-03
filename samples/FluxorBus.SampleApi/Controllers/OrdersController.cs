using FluxorBus.Abstractions;
using FluxorBus.SampleApi.Features.Orders;
using Microsoft.AspNetCore.Mvc;

namespace FluxorBus.SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController(IMessageBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder()
    {
        var order = new OrderCreated(Guid.NewGuid(), Random.Shared.Next(10, 500));

        await bus.PublishAsync(order);

        return Ok(new { order.OrderId });
    }
}