namespace Template.WebApp.Host.Areas.Api;

using Microsoft.AspNetCore.Antiforgery;

using Template.WebApp.Host.Application.Context;
using Template.WebApp.Host.Application.Telemetry;

[Area("api")]
[Route("[area]/[controller]/[action]")]
[ApiController]
[Authorize]
[RequireAntiforgeryToken]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
[ServiceFilter<RequestMetricsActionFilter>]
[ServiceFilter<ServiceContextResourceFilter>]
public abstract class BaseApiController : ControllerBase;
