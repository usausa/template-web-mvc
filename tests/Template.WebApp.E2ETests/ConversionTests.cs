namespace Template.WebApp;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

public sealed class ConversionTests : E2ETestBase
{
    [Fact]
    public async Task ValuesAreConvertedBeforeValidation()
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
        await Page.FillAsync("#Name", "ﾔﾏﾀﾞ Taro");
        await Page.Locator("#Name").BlurAsync();
        await Page.FillAsync("#Kana", "やまだﾀﾛｳ");
        await Page.Locator("#Kana").BlurAsync();
        await Page.FillAsync("#Code", "ＡＢ１２");
        await Page.Locator("#Code").BlurAsync();

        // Then
        await Expect(Page.Locator("#Name")).ToHaveValueAsync("ヤマダ　Ｔａｒｏ");
        await Expect(Page.Locator("#Kana")).ToHaveValueAsync("ヤマダタロウ");
        await Expect(Page.Locator("#Code")).ToHaveValueAsync("AB12");
        await Expect(Page.Locator("span[data-valmsg-for='Name']")).ToBeEmptyAsync();
        await Expect(Page.Locator("span[data-valmsg-for='Kana']")).ToBeEmptyAsync();

        // When
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "確認" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("変換した値で受け付けました")).ToBeVisibleAsync();
        await Expect(Page.Locator("#Name")).ToHaveValueAsync("ヤマダ　Ｔａｒｏ");
    }
}
