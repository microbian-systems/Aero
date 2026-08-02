namespace Aero.Web.Controllers;

/// <summary>
/// Represents a class for AeroWebBaseController.
/// </summary>
[Authorize]
public abstract class AeroWebBaseController(ILogger<AeroWebBaseController> log)
    : Controller
{
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<AeroWebBaseController> log = log;
}