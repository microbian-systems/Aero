namespace Aero.Web.Controllers;

[Authorize]
public abstract class AeroWebBaseController(ILogger<AeroWebBaseController> log)
    : Controller
{
    protected readonly ILogger<AeroWebBaseController> log = log;
}