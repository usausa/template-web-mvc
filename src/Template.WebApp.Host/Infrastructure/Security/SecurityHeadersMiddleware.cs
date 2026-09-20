namespace Template.WebApp.Host.Infrastructure.Security;

using Template.WebApp.Host.Settings;

// Security headers for every response. The CSP is enforced, or only reported (Csp:ReportOnly, Development)
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate next;

    private readonly bool reportOnly;

    // dotnet watch / Browser Link load their script from another localhost port and connect back to it
    private readonly string scriptSources;

    private readonly string connectSources;

    private readonly Func<object, Task> onStarting;

    public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment environment, CspSetting setting)
    {
        this.next = next;
        reportOnly = setting.ReportOnly;
        scriptSources = environment.IsDevelopment() ? "'self' http://localhost:*" : "'self'";
        connectSources = environment.IsDevelopment() ? "'self' http://localhost:* ws://localhost:* wss://localhost:*" : "'self'";
        onStarting = OnStarting;
    }

    public Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(onStarting, context);
        return next(context);
    }

    private Task OnStarting(object state)
    {
        var context = (HttpContext)state;
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // The nonce is for inline scripts in views (<script nonce="@Nonce.Value">). Bootstrap needs inline styles.
        var nonce = context.RequestServices.GetRequiredService<CspNonce>().Value;
        var policy = $"default-src 'self'; base-uri 'self'; object-src 'none'; form-action 'self'; frame-ancestors 'none'; img-src 'self' data:; font-src 'self'; style-src 'self' 'unsafe-inline'; script-src {scriptSources} 'nonce-{nonce}'; connect-src {connectSources}";
        if (reportOnly)
        {
            headers.ContentSecurityPolicyReportOnly = policy;
        }
        else
        {
            headers.ContentSecurityPolicy = policy;
        }

        return Task.CompletedTask;
    }
}
