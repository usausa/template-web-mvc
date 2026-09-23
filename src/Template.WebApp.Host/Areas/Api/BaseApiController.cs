namespace Template.WebApp.Host.Areas.Api;

using Template.WebApp.Host.Application.Context;
using Template.WebApp.Host.Application.Telemetry;

[Area("api")]
[Route("[area]/[controller]/[action]")]
[ApiController]
[Authorize]
[ServiceFilter<RequestMetricsActionFilter>]
[ServiceFilter<ServiceContextResourceFilter>]
public abstract class BaseApiController : ControllerBase;
