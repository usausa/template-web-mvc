namespace Template.WebApp;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class ValidationTests : E2ETestBase
{
    [Fact]
    public async Task ClientAndServerValidationShowMessages()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await Page.GotoAsync(factory.ServerAddress + "/account/login");
        await Page.FillAsync("#Name", "admin");
        await Page.FillAsync("#Password", "admin");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveTitleAsync(new Regex("ダッシュボード.*"));

        await Page.GotoAsync(factory.ServerAddress + "/sample/validation");
        var confirm = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "確認" });

        // When
        await Page.FillAsync("#Name", "abc");
        await Page.FillAsync("#Code", "ab");
        await confirm.ClickAsync();

        // Then
        await Expect(Page.Locator("span[data-valmsg-for='Name']")).ToHaveTextAsync("全角で入力してください");
        await Expect(Page.Locator("span[data-valmsg-for='Code']")).ToHaveTextAsync("半角英大文字と数字で入力してください");
        await Expect(Page.Locator("span[data-valmsg-for='Phone']")).ToBeEmptyAsync();

        // When
        await Page.FillAsync("#Name", "あいうえおかきくけこさ");
        await Page.Locator("#Name").BlurAsync();

        // Then
        await Expect(Page.Locator("span[data-valmsg-for='Name']")).ToHaveTextAsync("20バイト以内で入力してください");

        // When
        await Page.FillAsync("#Name", "山田");
        await Page.Locator("#Name").BlurAsync();
        await Page.FillAsync("#Code", "A1");
        await Page.Locator("#Code").BlurAsync();
        await confirm.ClickAsync();

        // Then
        await Expect(Page.Locator("span[data-valmsg-for='Phone']")).ToHaveTextAsync("いずれかを入力してください");
        await Expect(Page.Locator("span[data-valmsg-for='Mail']")).ToHaveTextAsync("いずれかを入力してください");

        // When
        await Page.FillAsync("#Phone", "03-1234-5678");
        await Page.Locator("#Phone").BlurAsync();
        await confirm.ClickAsync();

        // Then
        await Expect(Page.GetByText("入力内容は正しいです")).ToBeVisibleAsync();
    }
}
