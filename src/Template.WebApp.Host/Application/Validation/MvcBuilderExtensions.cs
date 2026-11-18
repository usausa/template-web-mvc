namespace Template.WebApp.Host.Application.Validation;

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddJapaneseMessages(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(static options =>
        {
            var provider = options.ModelBindingMessageProvider;
            provider.SetAttemptedValueIsInvalidAccessor(static (_, name) => Messages.MakeInvalid(name));
            provider.SetNonPropertyAttemptedValueIsInvalidAccessor(static _ => Messages.InvalidFormat);
            provider.SetUnknownValueIsInvalidAccessor(static name => Messages.MakeInvalid(name));
            provider.SetNonPropertyUnknownValueIsInvalidAccessor(static () => Messages.InvalidFormat);
            provider.SetValueIsInvalidAccessor(static _ => Messages.InvalidFormat);
            provider.SetValueMustNotBeNullAccessor(static _ => Messages.Required);
            provider.SetMissingBindRequiredValueAccessor(static _ => Messages.Required);
            provider.SetMissingKeyOrValueAccessor(static () => Messages.Required);
            provider.SetMissingRequestBodyRequiredValueAccessor(static () => Messages.RequestBodyRequired);
            provider.SetValueMustBeANumberAccessor(static _ => Messages.Number);
            provider.SetNonPropertyValueMustBeANumberAccessor(static () => Messages.Number);

            options.ModelMetadataDetailsProviders.Add(new DefaultErrorMessageProvider());
        });
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
