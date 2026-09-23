namespace Template.WebApp.Host.Application.Context;

using Microsoft.AspNetCore.Mvc.Filters;

// MVC の境界。リソースフィルターはモデルバインドやアクションより外側で動く
public sealed class ServiceContextResourceFilter : IAsyncResourceFilter
{
    private readonly ApplicationServiceContextProvider provider;

    public ServiceContextResourceFilter(ApplicationServiceContextProvider provider)
    {
        this.provider = provider;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        using var scope = provider.Begin(() => HttpServiceContext.GetOrCreate(context.HttpContext));
        await next();
    }
}
