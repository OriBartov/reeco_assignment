using Microsoft.AspNetCore.Mvc;
using OrderOps.Api.Models;
using OrderOps.Infrastructure.Services;

namespace OrderOps.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductListItemDto>>> List(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 1000);
        offset = Math.Max(offset, 0);

        var result = await productService.ListAsync(limit, offset, category, cancellationToken);
        return Ok(new PagedResponse<ProductListItemDto>(result.Data, result.Total, result.Limit, result.Offset));
    }
}
