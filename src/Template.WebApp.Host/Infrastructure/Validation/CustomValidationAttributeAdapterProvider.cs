namespace Template.WebApp.Host.Infrastructure.Validation;

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

using Template.WebApp.Infrastructure.Validation;

public sealed class CustomValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider defaultProvider = new();

    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer) =>
        attribute switch
        {
            Ms932LengthAttribute ms932Length => new Ms932LengthAttributeAdapter(ms932Length, stringLocalizer),
            CharacterTypeAttribute characterType => new CharacterTypeAttributeAdapter(characterType, stringLocalizer),
            _ => defaultProvider.GetAttributeAdapter(attribute, stringLocalizer)
        };
}
