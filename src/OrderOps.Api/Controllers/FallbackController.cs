using Microsoft.AspNetCore.Mvc;
using OrderOps.Api.Models;

namespace OrderOps.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class FallbackController : ControllerBase
{
    [Route("/api/{**catchAll}")]
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS")]
    public ActionResult<ErrorResponse> UnknownApiRoute()
    {
        return NotFound(new ErrorResponse("Not found.", "NOT_FOUND"));
    }
}
