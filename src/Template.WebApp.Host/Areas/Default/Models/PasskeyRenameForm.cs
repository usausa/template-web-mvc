#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Models;
#pragma warning restore CA1716

public sealed class PasskeyRenameForm
{
    [Required(ErrorMessage = Messages.Required)]
    public string Id { get; set; } = default!;

    [Required(ErrorMessage = Messages.Required)]
    [MaxLength(Length.Name, ErrorMessage = Messages.MaxLength)]
    public string Name { get; set; } = default!;
}
