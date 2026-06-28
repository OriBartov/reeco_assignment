using Microsoft.AspNetCore.Mvc;
using OrderOps.Infrastructure.Bulk;
using OrderOps.Infrastructure.Dtos;

namespace OrderOps.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public sealed class JobsController(IBulkJobService bulkJobService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobStatusDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await bulkJobService.GetJobAsync(id, cancellationToken);
        return Ok(job);
    }
}
