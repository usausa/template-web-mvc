namespace Template.WebApp;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class ConversionTests : E2ETestBase
{
    [Fact]
    public async Task ValuesAreConvertedOnServerAndBrowser()
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

        await Page.GotoAsync(factory.ServerAddress + "/sample/conversion");

        // When
        await Page.FillAsync("#ServerName", "ﾔﾏﾀﾞ Taro");
        await Page.FillAsync("#ServerKana", "やまだﾀﾛｳ");
        await Page.FillAsync("#ServerCode", "ＡＢ１２");
        await Page.FillAsync("#BrowserName", "ﾔﾏﾀﾞ Taro");
        await Page.Locator("#BrowserName").BlurAsync();
        await Page.FillAsync("#BrowserKana", "やまだﾀﾛｳ");
        await Page.Locator("#BrowserKana").BlurAsync();
        await Page.FillAsync("#BrowserCode", "ＡＢ１２");
        await Page.Locator("#BrowserCode").BlurAsync();

        // Then
        await Expect(Page.Locator("#ServerName")).ToHaveValueAsync("ﾔﾏﾀﾞ Taro");
        await Expect(Page.Locator("#BrowserName")).ToHaveValueAsync("ヤマダ　Ｔａｒｏ");
        await Expect(Page.Locator("#BrowserKana")).ToHaveValueAsync("ヤマダタロウ");
        await Expect(Page.Locator("#BrowserCode")).ToHaveValueAsync("AB12");
        await Expect(Page.Locator("span[data-valmsg-for='BrowserName']")).ToBeEmptyAsync();
        await Expect(Page.Locator("span[data-valmsg-for='BrowserKana']")).ToBeEmptyAsync();

        // When
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "確認" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("変換した値で受け付けました")).ToBeVisibleAsync();
        await Expect(Page.Locator("#ServerName")).ToHaveValueAsync("ヤマダ　Ｔａｒｏ");
        await Expect(Page.Locator("#ServerCode")).ToHaveValueAsync("AB12");
        await Expect(Page.Locator("#BrowserName")).ToHaveValueAsync("ヤマダ　Ｔａｒｏ");
    }
}
