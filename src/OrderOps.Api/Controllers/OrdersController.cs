using Microsoft.AspNetCore.Mvc;
using OrderOps.Api.Models;
using OrderOps.Infrastructure.Dtos;
using OrderOps.Infrastructure.Services;

namespace OrderOps.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderListItemDto>>> List(
        [FromQuery] OrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await orderService.ListAsync(query.Normalize(), cancellationToken);
        return Ok(new PagedResponse<OrderListItemDto>(result.Data, result.Total, result.Limit, result.Offset));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailDto>> GetById(string id, CancellationToken cancellationToken = default)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<OrderDetailDto>> Update(
        string id,
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await orderService.UpdateAsync(id, request, cancellationToken);
        return Ok(order);
    }
}
