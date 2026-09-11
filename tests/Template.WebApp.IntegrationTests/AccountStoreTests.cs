namespace Template.WebApp;

using System.Globalization;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Template.WebApp.Host.Application;
using Template.WebApp.Models.Entity;

// EF Coreを使わない利用者ストア(AccountStore)をUserManager経由で検証する
public sealed class AccountStoreTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public AccountStoreTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task CreateFindUpdateDelete()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AccountEntity>>();
        var name = $"user-{Guid.NewGuid():N}";

        // Act (作成: ロール・正規化名・スタンプはストアとUserManagerが補う)
        var user = new AccountEntity { Name = name };
        var created = await userManager.CreateAsync(user, "pass");

        // Assert
        Assert.True(created.Succeeded);
        Assert.True(user.Id > 0);
        Assert.Equal(Roles.User, user.Role);
        Assert.Equal(name.ToUpperInvariant(), user.NormalizedName);
        Assert.False(String.IsNullOrEmpty(user.SecurityStamp));

        // Act (検索は大文字小文字を区別しない)
        var found = await userManager.FindByNameAsync(name.ToUpperInvariant());

        // Assert
        Assert.NotNull(found);
        Assert.Equal(user.Id, found.Id);
        Assert.True(await userManager.CheckPasswordAsync(found, "pass"));
        Assert.False(await userManager.CheckPasswordAsync(found, "wrong"));

        // Act (更新: パスワード変更はスタンプも更新する)
        var changed = await userManager.ChangePasswordAsync(found, "pass", "newpass");
        var reloaded = await userManager.FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture));

        // Assert
        Assert.True(changed.Succeeded);
        Assert.NotNull(reloaded);
        Assert.True(await userManager.CheckPasswordAsync(reloaded, "newpass"));
        Assert.NotEqual(user.SecurityStamp, reloaded.SecurityStamp);

        // Act (削除)
        var deleted = await userManager.DeleteAsync(reloaded);

        // Assert
        Assert.True(deleted.Succeeded);
        Assert.Null(await userManager.FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture)));
    }

    [Fact]
    public async Task CreateDuplicateNameIsRejected()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AccountEntity>>();
        var name = $"user-{Guid.NewGuid():N}";
        await userManager.CreateAsync(new AccountEntity { Name = name }, "pass");

        // Act (大文字小文字違いも重複とみなす)
        var result = await userManager.CreateAsync(new AccountEntity { Name = name.ToUpperInvariant() }, "pass");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, static e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName));
    }

    [Fact]
    public async Task AccessFailedCountIsPersisted()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AccountEntity>>();
        var user = new AccountEntity { Name = $"user-{Guid.NewGuid():N}" };
        await userManager.CreateAsync(user, "pass");

        // Act
        await userManager.AccessFailedAsync(user);
        var reloaded = await userManager.FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture));

        // Assert
        Assert.NotNull(reloaded);
        Assert.Equal(1, await userManager.GetAccessFailedCountAsync(reloaded));
        Assert.False(await userManager.IsLockedOutAsync(reloaded));

        // Act (リセット)
        await userManager.ResetAccessFailedCountAsync(reloaded);
        reloaded = await userManager.FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture));

        // Assert
        Assert.NotNull(reloaded);
        Assert.Equal(0, await userManager.GetAccessFailedCountAsync(reloaded));
    }

    [Fact]
    public async Task PasskeyRoundTrip()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AccountEntity>>();
        var user = new AccountEntity { Name = $"user-{Guid.NewGuid():N}" };
        await userManager.CreateAsync(user, "pass");

        var credentialId = Guid.NewGuid().ToByteArray();
        var passkey = new UserPasskeyInfo(
            credentialId,
            publicKey: Guid.NewGuid().ToByteArray(),
            createdAt: DateTimeOffset.UtcNow,
            signCount: 1,
            transports: ["internal"],
            isUserVerified: true,
            isBackupEligible: false,
            isBackedUp: false,
            attestationObject: Guid.NewGuid().ToByteArray(),
            clientDataJson: Guid.NewGuid().ToByteArray())
        {
            Name = "Test"
        };

        // Act (追加)
        var added = await userManager.AddOrUpdatePasskeyAsync(user, passkey);
        var stored = Assert.Single(await userManager.GetPasskeysAsync(user));
        var owner = await userManager.FindByPasskeyIdAsync(credentialId);

        // Assert
        Assert.True(added.Succeeded);
        Assert.Equal(credentialId, stored.CredentialId);
        Assert.Equal("Test", stored.Name);
        Assert.Equal(passkey.PublicKey, stored.PublicKey);
        Assert.Equal(passkey.SignCount, stored.SignCount);
        Assert.Equal(passkey.Transports, stored.Transports);
        Assert.Equal(passkey.AttestationObject, stored.AttestationObject);
        Assert.Equal(passkey.ClientDataJson, stored.ClientDataJson);
        Assert.NotNull(owner);
        Assert.Equal(user.Id, owner.Id);

        // Act (更新: 同じ資格情報IDは上書きされ、重複しない)
        stored.Name = "Renamed";
        await userManager.AddOrUpdatePasskeyAsync(user, stored);
        var renamed = Assert.Single(await userManager.GetPasskeysAsync(user));

        // Assert
        Assert.Equal("Renamed", renamed.Name);

        // Act (削除)
        await userManager.RemovePasskeyAsync(user, credentialId);

        // Assert
        Assert.Empty(await userManager.GetPasskeysAsync(user));
        Assert.Null(await userManager.GetPasskeyAsync(user, credentialId));
        Assert.Null(await userManager.FindByPasskeyIdAsync(credentialId));
    }

    [Fact]
    public async Task PasskeyOfOtherUserIsNotFound()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AccountEntity>>();
        var owner = new AccountEntity { Name = $"user-{Guid.NewGuid():N}" };
        var other = new AccountEntity { Name = $"user-{Guid.NewGuid():N}" };
        await userManager.CreateAsync(owner, "pass");
        await userManager.CreateAsync(other, "pass");

        var credentialId = Guid.NewGuid().ToByteArray();
        var passkey = new UserPasskeyInfo(credentialId, [], DateTimeOffset.UtcNow, 0, null, false, false, false, [], []);
        await userManager.AddOrUpdatePasskeyAsync(owner, passkey);

        // Act
        var result = await userManager.GetPasskeyAsync(other, credentialId);

        // Assert
        Assert.Null(result);
        Assert.Empty(await userManager.GetPasskeysAsync(other));
    }
}
