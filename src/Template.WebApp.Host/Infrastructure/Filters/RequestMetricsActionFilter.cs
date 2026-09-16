namespace Template.WebApp.Host.Infrastructure.Filters;

using Microsoft.AspNetCore.Mvc.Filters;

using Template.WebApp.Host.Application.Telemetry;

public sealed class RequestMetricsActionFilter : IAsyncActionFilter
{
    private readonly ILogger<RequestMetricsActionFilter> log;

    private readonly TimeProvider timeProvider;

    private readonly TimeSpan longExecutionThreshold;

    private readonly ApplicationInstrument instrument;

    public RequestMetricsActionFilter(
        ILogger<RequestMetricsActionFilter> log,
        TimeProvider timeProvider,
        TelemetrySetting setting,
        ApplicationInstrument instrument)
    {
        this.log = log;
        this.timeProvider = timeProvider;
        longExecutionThreshold = TimeSpan.FromMilliseconds(setting.LongExecutionThreshold);
        this.instrument = instrument;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var route = (context.HttpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? context.HttpContext.Request.Path.Value ?? string.Empty;
        instrument.IncrementRequestExecution(method, route);

        var start = timeProvider.GetTimestamp();
        try
        {
            await next();
        }
        finally
        {
            var elapsed = timeProvider.GetElapsedTime(start);
            if (elapsed >= longExecutionThreshold)
            {
                instrument.IncrementRequestLongExecution(method, route);
                log.WarnLongExecution(method, route, (long)elapsed.TotalMilliseconds);
            }
        }
    }
}
