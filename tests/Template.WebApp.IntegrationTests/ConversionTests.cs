namespace Template.WebApp;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

public sealed class ConversionTests : IClassFixture<AuthDisabledApplicationFactory>
{
    private static readonly Uri ConversionUri = new("/sample/conversion", UriKind.Relative);

    private static readonly string[] ServerFields = ["ServerName", "ServerKana", "ServerCode"];

    private readonly AuthDisabledApplicationFactory factory;

    public ConversionTests(AuthDisabledApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task ServerFieldsHaveNoClientRules()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var document = await GetAsync(client);

        // Assert
        Assert.All(ServerFields, id =>
        {
            Assert.NotNull(document.QuerySelector($"input#{id}"));
            Assert.Null(GetAttribute(document, id, "data-val"));
            Assert.Null(GetAttribute(document, id, "data-kana-convert"));
        });
    }

    [Fact]
    public async Task BrowserFieldsHaveConvertOptionsAndClientRules()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var document = await GetAsync(client);

        // Assert
        Assert.Equal("Wide", GetAttribute(document, "BrowserName", "data-kana-convert"));
        Assert.Equal("HankanaToKatakana, HiraganaToKatakana", GetAttribute(document, "BrowserKana", "data-kana-convert"));
        Assert.Equal("RomanToNarrow, NumericToNarrow", GetAttribute(document, "BrowserCode", "data-kana-convert"));
        Assert.NotNull(GetAttribute(document, "BrowserName", "data-val-charactertype"));
        Assert.NotNull(GetAttribute(document, "BrowserName", "data-val-ms932length"));
        Assert.NotNull(GetAttribute(document, "BrowserKana", "data-val-charactertype"));
        Assert.NotNull(GetAttribute(document, "BrowserCode", "data-val-charactertype"));
    }

    [Fact]
    public async Task ConvertedValuesAreAccepted()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = GetToken(await GetAsync(client));

        // Act
        var document = await PostAsync(client, new Dictionary<string, string>
        {
            ["ServerName"] = "ﾔﾏﾀﾞ Taro",
            ["ServerKana"] = "やまだﾀﾛｳ",
            ["ServerCode"] = "ＡＢ１２",
            ["BrowserName"] = "ﾔﾏﾀﾞ Taro",
            ["BrowserKana"] = "やまだﾀﾛｳ",
            ["BrowserCode"] = "ＡＢ１２",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Contains("変換した値で受け付けました", document.Body!.TextContent, StringComparison.Ordinal);
        Assert.Equal("ヤマダ　Ｔａｒｏ", GetAttribute(document, "ServerName", "value"));
        Assert.Equal("ヤマダタロウ", GetAttribute(document, "ServerKana", "value"));
        Assert.Equal("AB12", GetAttribute(document, "ServerCode", "value"));
        Assert.Equal("ヤマダ　Ｔａｒｏ", GetAttribute(document, "BrowserName", "value"));
        Assert.Equal("ヤマダタロウ", GetAttribute(document, "BrowserKana", "value"));
        Assert.Equal("AB12", GetAttribute(document, "BrowserCode", "value"));
    }

    [Fact]
    public async Task UnacceptableConvertedValueShowsMessage()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = GetToken(await GetAsync(client));

        // Act
        var document = await PostAsync(client, new Dictionary<string, string>
        {
            ["ServerCode"] = "ａｂ１２",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Equal("半角英大文字と数字で入力してください", GetMessage(document, "ServerCode"));
        Assert.Equal("ab12", GetAttribute(document, "ServerCode", "value"));
        Assert.DoesNotContain("変換した値で受け付けました", document.Body!.TextContent, StringComparison.Ordinal);
    }

    private static async Task<IHtmlDocument> GetAsync(HttpClient client)
    {
        var response = await client.GetAsync(ConversionUri, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static async Task<IHtmlDocument> PostAsync(HttpClient client, Dictionary<string, string> values)
    {
        using var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync(ConversionUri, content, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static string GetToken(IHtmlDocument document) =>
        document.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;

    private static string? GetAttribute(IHtmlDocument document, string id, string name) =>
        document.QuerySelector($"input#{id}")?.GetAttribute(name);

    private static string? GetMessage(IHtmlDocument document, string name) =>
        document.QuerySelector($"span[data-valmsg-for='{name}']")?.TextContent;
}
