namespace Template.WebApp.Host.Infrastructure.Logging;

public sealed class LoggingContextMiddleware
{
    private readonly RequestDelegate next;

    public LoggingContextMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        LoggingContext.Set(context.Connection.RemoteIpAddress?.ToString(), context.User.FindFirstValue(ClaimTypes.NameIdentifier));
        try
        {
            await next(context);
        }
        finally
        {
            LoggingContext.Clear();
        }
    }
}
