namespace Template.WebApp.Host.Infrastructure.Validation;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

using Template.WebApp.Infrastructure.Validation;

public sealed class CharacterTypeAttributeAdapter : AttributeAdapterBase<CharacterTypeAttribute>
{
    public CharacterTypeAttributeAdapter(CharacterTypeAttribute attribute, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-charactertype", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-charactertype-pattern", Attribute.Pattern);
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext) =>
        GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
}
