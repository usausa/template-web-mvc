namespace Template.WebApp.Host.Areas.Default.Models;

using Smart.ComponentModel.DataAnnotations;

[AnyRequired(nameof(Phone), nameof(Mail))]
public sealed class ValidationSampleForm
{
    [Ms932MaxLength(20)]
    [FullWidth]
    public string? Name { get; set; }

    [UpperAlphanumeric]
    public string? Code { get; set; }

    [NumericHyphen]
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
