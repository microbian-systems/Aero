using Aero.Core;
using Aero.Core.Railway;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;
using static Aero.Core.Railway.Prelude;

namespace Aero.Web.Core.Controllers;

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

    protected IActionResult HandleResult<TError, TValue>(Result<TError, TValue> result)
    {
        return result switch
        {
            Result<TError, TValue>.Ok(var value) => Ok(value),
            Result<TError, TValue>.Failure(AeroError.NotFound nf) => NotFound(nf.msg),
            Result<TError, TValue>.Failure(AeroError.Validation v) => BadRequest(v.Errors),
            Result<TError, TValue>.Failure(AeroError.Unauthorized) => Unauthorized(),
            Result<TError, TValue>.Failure(AeroError.Conflict c) => Conflict(c.msg),
            Result<TError, TValue>.Failure(AeroError.BadRequest c) => BadRequest(c.msg),
            Result<TError, TValue>.Failure(AeroError.NotAllowed c) => Problem(c.msg),
            Result<TError, TValue>.Failure(AeroError.Forbidden c) => Forbid(c.msg),
            Result<TError, TValue>.Failure(AeroError.Exists c) => Problem(c.msg),

            _ => Problem($"error: {((TError)result)}")
        };

    }
}
