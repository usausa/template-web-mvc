namespace Template.WebApp.Host.Infrastructure.Validation;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

using Template.WebApp.Infrastructure.Validation;

public sealed class Ms932LengthAttributeAdapter : AttributeAdapterBase<Ms932LengthAttribute>
{
    public Ms932LengthAttributeAdapter(Ms932LengthAttribute attribute, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-ms932length", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-ms932length-min", Attribute.MinimumLength.ToString(CultureInfo.InvariantCulture));
        MergeAttribute(context.Attributes, "data-val-ms932length-max", Attribute.MaximumLength.ToString(CultureInfo.InvariantCulture));
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext) =>
        GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName(), Attribute.MaximumLength, Attribute.MinimumLength);
}
