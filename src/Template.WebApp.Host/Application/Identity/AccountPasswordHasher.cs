namespace Template.WebApp.Host.Application.Identity;

using Microsoft.AspNetCore.Identity;

using Template.WebApp.Infrastructure.Security;

// Identityのハッシャーを既存のIPasswordProviderへ委譲する。ハッシュ形式(byte[])はBase64で往復させ、既存データと互換を保つ
public sealed class AccountPasswordHasher : IPasswordHasher<AccountEntity>
{
    private readonly IPasswordProvider passwordProvider;

    public AccountPasswordHasher(IPasswordProvider passwordProvider)
    {
        this.passwordProvider = passwordProvider;
    }

    public string HashPassword(AccountEntity user, string password) =>
        Convert.ToBase64String(passwordProvider.Generate(password));

    public PasswordVerificationResult VerifyHashedPassword(AccountEntity user, string hashedPassword, string providedPassword) =>
        passwordProvider.Match(providedPassword, Convert.FromBase64String(hashedPassword))
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
}
