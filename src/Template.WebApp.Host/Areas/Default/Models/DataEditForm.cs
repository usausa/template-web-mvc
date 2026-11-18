namespace Template.WebApp.Host.Areas.Default.Models;

public sealed class DataEditForm
{
    [Required]
    [StringLength(Length.Name)]
    public string Name { get; set; } = default!;

    [Required]
    [Range(0, 999_999_999)]
    public int? Value { get; set; }
}
