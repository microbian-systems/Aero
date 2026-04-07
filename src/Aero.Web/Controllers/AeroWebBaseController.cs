using Aero.Core;
using Aero.Core.Railway;


namespace Aero.Web.Core.Controllers;

[Authorize]
public abstract class AeroWebBaseController(ILogger<AeroWebBaseController> log)
    : Controller
{
    protected readonly ILogger<AeroWebBaseController> log = log;
}