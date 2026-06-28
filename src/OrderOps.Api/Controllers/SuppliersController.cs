using Microsoft.AspNetCore.Mvc;
using OrderOps.Api.Models;
using OrderOps.Infrastructure.Dtos;
using OrderOps.Infrastructure.Services;

namespace OrderOps.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public sealed class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<SupplierListItemDto>>> List(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 1000);
        offset = Math.Max(offset, 0);

        var result = await supplierService.ListAsync(limit, offset, cancellationToken);
        return Ok(new PagedResponse<SupplierListItemDto>(result.Data, result.Total, result.Limit, result.Offset));
    }

    [HttpGet("{id}/performance")]
    public async Task<ActionResult<SupplierPerformanceDto>> GetPerformance(
        string id,
        CancellationToken cancellationToken = default)
    {
        var performance = await supplierService.GetPerformanceAsync(id, cancellationToken);
        return Ok(performance);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDetailDto>> GetById(string id, CancellationToken cancellationToken = default)
    {
        var supplier = await supplierService.GetByIdAsync(id, cancellationToken);
        return Ok(supplier);
    }
}
