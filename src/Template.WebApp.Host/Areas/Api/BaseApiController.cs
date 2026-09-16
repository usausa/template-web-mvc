namespace Template.WebApp.Host.Areas.Api;

using Template.WebApp.Host.Infrastructure.Filters;

[Area("api")]
[Route("[area]/[controller]/[action]")]
[ApiController]
[Authorize]
[ServiceFilter<RequestMetricsActionFilter>]
public abstract class BaseApiController : ControllerBase;
