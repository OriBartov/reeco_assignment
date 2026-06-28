using Microsoft.AspNetCore.Mvc;
using OrderOps.Api.Models;
using OrderOps.Infrastructure.Bulk;
using OrderOps.Infrastructure.Dtos;
using OrderOps.Infrastructure.Services;

namespace OrderOps.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(
    IOrderService orderService,
    IStatsService statsService,
    IAnomalyService anomalyService,
    IBulkJobService bulkJobService) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<OrderStatsDto>> GetStats(CancellationToken cancellationToken = default)
    {
        var stats = await statsService.GetOrderStatsAsync(cancellationToken);
        return Ok(stats);
    }

    [HttpGet("anomalies")]
    public async Task<ActionResult<AnomaliesListDto>> GetAnomalies(CancellationToken cancellationToken = default)
    {
        var anomalies = await anomalyService.GetAnomaliesAsync(cancellationToken);
        return Ok(new AnomaliesListDto(anomalies));
    }

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

    [HttpPost("bulk-action")]
    public async Task<ActionResult<BulkActionJobResponse>> BulkAction(
        [FromBody] BulkActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await bulkJobService.EnqueueBulkActionAsync(
            request.OrderIds ?? [],
            request.Action ?? string.Empty,
            request.Reason,
            cancellationToken);

        return Accepted(response);
    }

    [HttpPost("bulk-actions")]
    public async Task<ActionResult<BulkActionsJobResponse>> BulkActions(
        [FromBody] BulkActionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await bulkJobService.EnqueueBulkActionAsync(
            request.OrderIds ?? [],
            request.Action ?? string.Empty,
            request.Reason,
            cancellationToken);

        return Accepted(new BulkActionsJobResponse(response.JobId));
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkJobResponse>> Bulk(
        [FromBody] BulkRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await bulkJobService.EnqueueBulkActionAsync(
            request.OrderIds ?? [],
            request.Action ?? string.Empty,
            request.Reason,
            cancellationToken);

        return Accepted(new BulkJobResponse(response.JobId));
    }
}
