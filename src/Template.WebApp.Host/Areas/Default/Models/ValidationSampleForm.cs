namespace Template.WebApp.Host.Areas.Default.Models;

using BunnyTail.MemberAccessor;

using Template.WebApp.Infrastructure.Validation;

[GenerateAccessor]
[AnyRequired(nameof(Phone), nameof(Mail))]
public sealed class ValidationSampleForm
{
    [Ms932Length(20)]
    [CharacterType(CharacterType.FullWidth)]
    public string? Name { get; set; }

    [CharacterType(CharacterType.UpperAlphanumeric)]
    public string? Code { get; set; }

    [CharacterType(CharacterType.NumericHyphen)]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Mail { get; set; }

    public bool UsePeriod { get; set; }

    [RequiredWhen(nameof(UsePeriod))]
    public DateOnly? From { get; set; }

    [RequiredWhen(nameof(UsePeriod))]
    [CompareTo(CompareToOperation.GreaterThanOrEqual, nameof(From))]
    public DateOnly? To { get; set; }
}
