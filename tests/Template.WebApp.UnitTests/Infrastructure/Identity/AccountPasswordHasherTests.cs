namespace Template.WebApp.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;

using Template.WebApp.Host.Infrastructure.Identity;
using Template.WebApp.Infrastructure.Security;
using Template.WebApp.Models.Entity;

public sealed class AccountPasswordHasherTests
{
    [Fact]
    public void VerifyHashedPasswordReturnsSuccess()
    {
        // Arrange
        var hasher = new AccountPasswordHasher(new DefaultPasswordProvider(new DefaultPasswordProviderOptions()));
        var user = new AccountEntity();
        var hash = hasher.HashPassword(user, "password");

        // Act
        var result = hasher.VerifyHashedPassword(user, hash, "password");

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void VerifyHashedPasswordWrongPasswordReturnsFailed()
    {
        // Arrange
        var hasher = new AccountPasswordHasher(new DefaultPasswordProvider(new DefaultPasswordProviderOptions()));
        var user = new AccountEntity();
        var hash = hasher.HashPassword(user, "password");

        // Act
        var result = hasher.VerifyHashedPassword(user, hash, "wrong");

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void VerifyHashedPasswordAcceptsProviderHash()
    {
        // Arrange (IPasswordProviderが直接生成した既存データ形式と互換であること)
        var provider = new DefaultPasswordProvider(new DefaultPasswordProviderOptions());
        var hasher = new AccountPasswordHasher(provider);
        var hash = Convert.ToBase64String(provider.Generate("password"));

        // Act
        var result = hasher.VerifyHashedPassword(new AccountEntity(), hash, "password");

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }
}
