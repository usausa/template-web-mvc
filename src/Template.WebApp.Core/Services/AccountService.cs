namespace Template.WebApp.Services;

using Template.WebApp.Accessors;
using Template.WebApp.Infrastructure.Security;

// テーブル作成と初期アカウントの投入。認証そのものはIdentity Core(SignInManager)が担う
public sealed class AccountService
{
    private readonly AccountAccessor accountAccessor;

    private readonly IPasswordProvider passwordProvider;

    private readonly ServiceContextProvider contextProvider;

    public AccountService(
        AccountAccessor accountAccessor,
        IPasswordProvider passwordProvider,
        ServiceContextProvider contextProvider)
    {
        this.accountAccessor = accountAccessor;
        this.passwordProvider = passwordProvider;
        this.contextProvider = contextProvider;
    }

    // Seed initial account
    public async ValueTask InitializeAsync(InitialAccountOption option, string role)
    {
        var count = await accountAccessor.CountAsync();
        if (count == 0)
        {
            var context = contextProvider.Current;
            await accountAccessor.InsertAsync(
                option.Id,
                option.Id.ToUpperInvariant(),
                passwordProvider.Generate(option.Password),
                role,
                Guid.NewGuid().ToString("N"),
                context.Now.DateTime);
        }
    }
}
