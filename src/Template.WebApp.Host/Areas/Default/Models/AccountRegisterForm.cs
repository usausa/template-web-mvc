#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Models;
#pragma warning restore CA1716

public sealed class AccountRegisterForm
{
    [Required(ErrorMessage = Messages.Required)]
    [MaxLength(Length.Name, ErrorMessage = Messages.MaxLength)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = Messages.Required)]
    [MaxLength(Length.Password, ErrorMessage = Messages.MaxLength)]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = Messages.Required)]
    [Compare(nameof(Password), ErrorMessage = Messages.PasswordMismatch)]
    public string ConfirmPassword { get; set; } = default!;
}
