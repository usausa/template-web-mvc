namespace Template.WebApp.Host.Application.Validation;

using Microsoft.Extensions.Localization;

public sealed class MessageLocalizer : IStringLocalizer
{
    private static readonly Dictionary<string, string> Values = new(StringComparer.Ordinal)
    {
        ["RequiredAttribute_Error"] = Messages.Required,
        ["RequiredWhenAttribute_Error"] = Messages.Required,
        ["EmptyWhenAttribute_Error"] = Messages.Empty,
        ["StringLengthAttribute_Error"] = Messages.MaxLength,
        ["MaxLengthAttribute_Error"] = Messages.MaxLength,
        ["RangeAttribute_Error"] = Messages.Range,
        ["EmailAddressAttribute_Error"] = Messages.EmailAddress,
        ["AnyRequiredAttribute_Error"] = Messages.AnyRequired,
        ["AnyRequiredWhenAttribute_Error"] = Messages.AnyRequired,
        ["Ms932MaxLengthAttribute_Error"] = Messages.ByteMaxLength,
        ["Ms932LengthAttribute_Error"] = Messages.ByteLength,
        ["Ms932FixedLengthAttribute_Error"] = Messages.ByteFixedLength,
        ["Ms932EncodableAttribute_Error"] = Messages.Ms932Encodable,
        ["NoHalfWidthKanaAttribute_Error"] = Messages.NoHalfWidthKana,
        ["FullWidthAttribute_Error"] = Messages.FullWidth,
        ["UpperAlphanumericAttribute_Error"] = Messages.UpperAlphanumeric,
        ["UpperAlphanumericHyphenAttribute_Error"] = Messages.UpperAlphanumericHyphen,
        ["AlphanumericAttribute_Error"] = Messages.Alphanumeric,
        ["AlphanumericHyphenAttribute_Error"] = Messages.AlphanumericHyphen,
        ["NumericAttribute_Error"] = Messages.Numeric,
        ["NumericHyphenAttribute_Error"] = Messages.NumericHyphen,
        ["KatakanaAttribute_Error"] = Messages.Katakana,
        ["HiraganaAttribute_Error"] = Messages.Hiragana,
        ["CompareToAttribute_Error"] = Messages.Compare,
        ["CompareToWhenAttribute_Error"] = Messages.Compare,
        ["EqualToAttribute_Error"] = Messages.EqualTo,
        ["ElementRequiredAttribute_Error"] = Messages.ElementRequired,
        ["MaxCountAttribute_Error"] = Messages.MaxCount,
        ["DuplicateKeyAttribute_Error"] = Messages.DuplicateKey,
        ["AttemptedValueIsInvalid_Error"] = Messages.AttemptedValueIsInvalid,
        ["NonPropertyAttemptedValueIsInvalid_Error"] = Messages.InvalidFormat,
        ["UnknownValueIsInvalid_Error"] = Messages.UnknownValueIsInvalid,
        ["NonPropertyUnknownValueIsInvalid_Error"] = Messages.InvalidFormat,
        ["ValueIsInvalid_Error"] = Messages.InvalidFormat,
        ["ValueMustNotBeNull_Error"] = Messages.Required,
        ["MissingBindRequiredValue_Error"] = Messages.Required,
        ["MissingKeyOrValue_Error"] = Messages.Required,
        ["MissingRequestBodyRequiredValue_Error"] = Messages.RequestBodyRequired,
        ["ValueMustBeANumber_Error"] = Messages.Number,
        ["NonPropertyValueMustBeANumber_Error"] = Messages.Number
    };

    public LocalizedString this[string name] =>
        Values.TryGetValue(name, out var value) ? new LocalizedString(name, value) : new LocalizedString(name, name, resourceNotFound: true);

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = this[name];
            return new LocalizedString(name, String.Format(CultureInfo.CurrentCulture, value.Value, arguments), value.ResourceNotFound);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        Values.Select(static x => new LocalizedString(x.Key, x.Value));
}
