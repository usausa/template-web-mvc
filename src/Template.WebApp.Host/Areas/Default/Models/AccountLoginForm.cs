#pragma warning disable CA1716
namespace Template.WebApp.Host.Areas.Default.Models;
#pragma warning restore CA1716

using Template.WebApp.Host.Infrastructure.Identity;

public sealed class AccountLoginForm
{
    [Required(ErrorMessage = Messages.Required)]
    public string Name { get; set; } = default!;

    // パスキーでのログイン時は空で送られる
    public string? Password { get; set; }

    public bool RememberMe { get; set; }

    // passkey-submit.jsが組み立てる。値があればパスキーでのログインとして扱う
    public PasskeyInputModel? Passkey { get; set; }
}
