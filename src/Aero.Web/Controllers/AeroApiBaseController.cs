using System.Net.Mime;
using Aero.Core;
using Aero.Core.Railway;
using Microsoft.AspNetCore.RateLimiting;
using static Aero.Core.Railway.Prelude;

namespace Aero.Web.Controllers;

[Authorize] 
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[EnableRateLimiting("api")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status202Accepted)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status304NotModified)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public abstract class AeroApiBaseController(ILogger<AeroApiBaseController> log)
    : ControllerBase
{
    protected readonly ILogger<AeroApiBaseController> log = log;

    protected Option<long> GetUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id");

        if (claim is null)
        {
            log.LogWarning("User does not have a id claim");
            return None;
        }

        return long.TryParse(claim.Value, out var id)
            ? Some(id)
            : None;
    }

    protected IActionResult HandleResult<T>(Result<T, AeroError> result)
    {
        return result switch
        {
            Result<T, AeroError>.Ok(var value) => Ok(value),
            Result<T, AeroError>.Failure(AeroError.NotFound nf) => NotFound(nf.msg),
            Result<T, AeroError>.Failure(AeroError.Validation v) => BadRequest(v.Errors),
            Result<T, AeroError>.Failure(AeroError.Unauthorized) => Unauthorized(),
            Result<T, AeroError>.Failure(AeroError.Conflict c) => Conflict(c.msg),
            Result<T, AeroError>.Failure(AeroError.BadRequest c) => BadRequest(c.msg),
            Result<T, AeroError>.Failure(AeroError.NotAllowed c) => Problem(c.msg),
            Result<T, AeroError>.Failure(AeroError.Forbidden c) => Forbid(),
            Result<T, AeroError>.Failure(AeroError.Exists c) => Problem(c.msg),

            _ => Problem("Unexpected error")
        };

    }
}
