#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Models;
#pragma warning restore CA1716

public sealed class AccountRegisterForm
{
    [Required]
    [MaxLength(Length.Name)]
    public string Name { get; set; } = default!;

    [Required]
    [MaxLength(Length.Password)]
    public string Password { get; set; } = default!;

    [Required]
    [Compare(nameof(Password), ErrorMessage = Messages.PasswordMismatch)]
    public string ConfirmPassword { get; set; } = default!;
}
