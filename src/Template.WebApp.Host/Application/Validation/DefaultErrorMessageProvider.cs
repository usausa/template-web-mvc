namespace Template.WebApp.Host.Application.Validation;

using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

using Template.WebApp.Infrastructure.Validation;

public sealed class DefaultErrorMessageProvider : IValidationMetadataProvider
{
    private static readonly string? EmailAddressDefaultMessage = new EmailAddressAttribute().ErrorMessage;

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if (attribute is RequiredOrEmptyAttribute requiredOrEmpty)
            {
                requiredOrEmpty.EmptyErrorMessage ??= Messages.Empty;
            }

            if ((attribute.ErrorMessageResourceType is null) && IsDefaultMessage(attribute) && (ResolveMessage(attribute) is { } message))
            {
                attribute.ErrorMessage = message;
            }
        }
    }

    private static bool IsDefaultMessage(ValidationAttribute attribute) =>
        attribute is EmailAddressAttribute
            ? String.Equals(attribute.ErrorMessage, EmailAddressDefaultMessage, StringComparison.Ordinal)
            : attribute.ErrorMessage is null;

    private static string? ResolveMessage(ValidationAttribute attribute) =>
        attribute switch
        {
            RequiredAttribute or RequiredWhenAttribute or RequiredOrEmptyAttribute => Messages.Required,
            StringLengthAttribute or MaxLengthAttribute => Messages.MaxLength,
            RangeAttribute => Messages.Range,
            EmailAddressAttribute => Messages.EmailAddress,
            Ms932LengthAttribute ms932Length => ResolveByteLengthMessage(ms932Length),
            CharacterTypeAttribute characterType => ResolveCharacterTypeMessage(characterType.CharacterType),
            CompareToAttribute or CompareToWhenAttribute => Messages.Compare,
            AnyRequiredAttribute or AnyRequiredWhenAttribute => Messages.AnyRequired,
            ElementRequiredAttribute => Messages.ElementRequired,
            MaxCountAttribute => Messages.MaxCount,
            DuplicateKeyAttribute => Messages.DuplicateKey,
            _ => null
        };

    private static string ResolveByteLengthMessage(Ms932LengthAttribute attribute) =>
        attribute.MinimumLength == 0
            ? Messages.ByteMaxLength
            : attribute.MinimumLength == attribute.MaximumLength
                ? Messages.ByteFixedLength
                : Messages.ByteLength;

    private static string? ResolveCharacterTypeMessage(CharacterType characterType) =>
        characterType switch
        {
            CharacterType.NoHalfWidthKana => Messages.NoHalfWidthKana,
            CharacterType.FullWidth => Messages.FullWidth,
            CharacterType.UpperAlphanumeric => Messages.UpperAlphanumeric,
            CharacterType.UpperAlphanumericHyphen => Messages.UpperAlphanumericHyphen,
            CharacterType.Alphanumeric => Messages.Alphanumeric,
            CharacterType.AlphanumericHyphen => Messages.AlphanumericHyphen,
            CharacterType.Numeric => Messages.Numeric,
            CharacterType.NumericHyphen => Messages.NumericHyphen,
            _ => null
        };
}
