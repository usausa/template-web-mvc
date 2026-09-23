namespace Template.WebApp.Host.Application.Identity;

// パスキー操作の結果をフォームで受け取るモデル(passkey-submit.jsが組み立てる)
public sealed class PasskeyInputModel
{
    public string? CredentialJson { get; set; }

    public string? Error { get; set; }
}
