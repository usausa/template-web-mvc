namespace Template.WebApp.Services;

using Template.WebApp.Accessors;
using Template.WebApp.Infrastructure.Security;

// テーブル作成と初期アカウントの投入。認証そのものはIdentity Core(SignInManager)が担う
public sealed class AccountService
{
    private readonly AccountAccessor accountAccessor;

    private readonly AccountPasskeyAccessor accountPasskeyAccessor;

    private readonly IPasswordProvider passwordProvider;

    private readonly TimeProvider timeProvider;

    public AccountService(
        AccountAccessor accountAccessor,
        AccountPasskeyAccessor accountPasskeyAccessor,
        IPasswordProvider passwordProvider,
        TimeProvider timeProvider)
    {
        this.accountAccessor = accountAccessor;
        this.accountPasskeyAccessor = accountPasskeyAccessor;
        this.passwordProvider = passwordProvider;
        this.timeProvider = timeProvider;
    }

    public async ValueTask InitializeAsync(string initialName, string initialPassword, string initialRole)
    {
        accountAccessor.Create();
        accountPasskeyAccessor.Create();

        // Seed initial account
        var count = await accountAccessor.CountAsync();
        if (count == 0)
        {
            await accountAccessor.InsertAsync(
                initialName,
                initialName.ToUpperInvariant(),
                passwordProvider.Generate(initialPassword),
                initialRole,
                Guid.NewGuid().ToString("N"),
                timeProvider.GetLocalNow().DateTime);
        }
    }
}
