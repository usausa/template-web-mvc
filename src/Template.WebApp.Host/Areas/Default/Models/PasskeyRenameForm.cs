#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Models;
#pragma warning restore CA1716

public sealed class PasskeyRenameForm
{
    [Required]
    public string Id { get; set; } = default!;

    [Required]
    [MaxLength(Length.Name)]
    public string Name { get; set; } = default!;
}
