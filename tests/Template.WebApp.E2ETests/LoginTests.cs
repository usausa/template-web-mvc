namespace Template.WebApp;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class LoginTests : E2ETestBase
{
    [Fact]
    public async Task LoginShowsDashboardPage()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        // When
        await Page.GotoAsync(factory.ServerAddress + "/");
        await Expect(Page).ToHaveURLAsync(new Regex(".*/account/login.*"));

        await Page.FillAsync("#Name", "admin");
        await Page.FillAsync("#Password", "admin");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン", Exact = true }).ClickAsync();

        // Then
        await Expect(Page).ToHaveTitleAsync(new Regex("ダッシュボード.*"));
    }

    [Fact]
    public async Task LoginWithWrongPasswordShowsError()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        // When
        await Page.GotoAsync(factory.ServerAddress + "/account/login");
        await Page.FillAsync("#Name", "admin");
        await Page.FillAsync("#Password", "wrong");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン", Exact = true }).ClickAsync();

        // Then
        await Expect(Page.GetByText("ログインに失敗しました")).ToBeVisibleAsync();
    }
}
