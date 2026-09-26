namespace Template.WebApp;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

// CDPのWebAuthn仮想認証器を使ってパスキーの登録〜ログインを検証する
public sealed class PasskeyTests : E2ETestBase
{
    private async Task EnableVirtualAuthenticatorAsync()
    {
        var session = await Page.Context.NewCDPSessionAsync(Page);
        await session.SendAsync("WebAuthn.enable");
        await session.SendAsync("WebAuthn.addVirtualAuthenticator", new Dictionary<string, object>
        {
            ["options"] = new Dictionary<string, object>
            {
                ["protocol"] = "ctap2",
                ["transport"] = "internal",
                ["hasResidentKey"] = true,
                ["hasUserVerification"] = true,
                ["isUserVerified"] = true,
                ["automaticPresenceSimulation"] = true
            }
        });
    }

    [Fact]
    public async Task RegisterPasskeyAndLogin()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        // WebAuthnのRP IDにIPアドレスは使えないため、127.0.0.1ではなくlocalhostでアクセスする
        var address = factory.ServerAddress.Replace("127.0.0.1", "localhost", StringComparison.Ordinal);

        await EnableVirtualAuthenticatorAsync();

        // パスワードでログイン
        await Page.GotoAsync(address + "/account/login");
        await Page.FillAsync("#Name", "admin");
        await Page.FillAsync("#Password", "admin");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveTitleAsync(new Regex("ダッシュボード.*"));

        // パスキーを登録(仮想認証器が自動応答)して名前を付ける
        await Page.GotoAsync(address + "/account/passkeys");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "パスキーを追加" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(".*/account/renamepasskey.*", RegexOptions.IgnoreCase));
        await Page.FillAsync("#Name", "E2E Passkey");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "保存" }).ClickAsync();
        await Expect(Page.GetByText("E2E Passkey")).ToBeVisibleAsync();

        // ログアウトしてログイン画面へ戻る
        await Page.Locator(".dropdown-toggle").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログアウト" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(".*/account/login.*", RegexOptions.IgnoreCase));

        // Then (仮想認証器では条件付きUI(自動フィル)によりパスキーログインが自動実行される)
        await Expect(Page).ToHaveTitleAsync(new Regex("ダッシュボード.*"), new PageAssertionsToHaveTitleOptions { Timeout = 30_000 });
    }
}
