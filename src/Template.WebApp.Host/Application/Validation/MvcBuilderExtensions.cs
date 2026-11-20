namespace Template.WebApp.Host.Application.Validation;

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Smart.ComponentModel.DataAnnotations.AspNetCore;

public static class MvcBuilderExtensions
{
    private static readonly MessageLocalizer Localizer = new();

    public static IMvcBuilder AddJapaneseMessages(this IMvcBuilder builder)
    {
        builder.AddDataAnnotationsLocalization(static options => options.DataAnnotationLocalizerProvider = static (_, _) => Localizer);
        builder.AddDefaultMessageLocalization();
        builder.AddJsonOptions(static options => options.AllowInputFormatterExceptionMessages = false);
        builder.ConfigureApiBehaviorOptions(static options => options.InvalidModelStateResponseFactory = CreateInvalidModelStateResponse);

        return builder;
    }

    private static BadRequestObjectResult CreateInvalidModelStateResponse(ActionContext context)
    {
        foreach (var entry in context.ModelState.Values)
        {
            for (var i = 0; i < entry.Errors.Count; i++)
            {
                if (String.IsNullOrEmpty(entry.Errors[i].ErrorMessage))
                {
                    entry.Errors[i] = new ModelError(Messages.InvalidFormat);
                }
            }
        }

        var problemDetails = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>().CreateValidationProblemDetails(context.HttpContext, context.ModelState);
        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json", "application/problem+xml" }
        };
    }
}
