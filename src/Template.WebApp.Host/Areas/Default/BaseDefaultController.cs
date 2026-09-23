#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default;
#pragma warning restore CA1716

using Template.WebApp.Host.Application.Context;

[Area("default")]
[Route("[controller]/[action]")]
[Authorize]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
[ApiExplorerSettings(IgnoreApi = true)]
[ServiceFilter<ServiceContextResourceFilter>]
public abstract class BaseDefaultController : Controller;
