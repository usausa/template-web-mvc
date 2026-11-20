namespace Template.WebApp;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

public sealed class ConversionTests : IClassFixture<AuthDisabledApplicationFactory>
{
    private static readonly Uri ConversionUri = new("/sample/conversion", UriKind.Relative);

    private readonly AuthDisabledApplicationFactory factory;

    public ConversionTests(AuthDisabledApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task PageRendersConvertOptions()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var document = await GetAsync(client);

        // Assert
        Assert.Equal("Wide", GetAttribute(document, "Name", "data-kana-convert"));
        Assert.Equal("HankanaToKatakana, HiraganaToKatakana", GetAttribute(document, "Kana", "data-kana-convert"));
        Assert.Equal("RomanToNarrow, NumericToNarrow", GetAttribute(document, "Code", "data-kana-convert"));
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
            ["Name"] = "ﾔﾏﾀﾞ Taro",
            ["Kana"] = "やまだﾀﾛｳ",
            ["Code"] = "ＡＢ１２",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Contains("変換した値で受け付けました", document.Body!.TextContent, StringComparison.Ordinal);
        Assert.Equal("ヤマダ　Ｔａｒｏ", GetAttribute(document, "Name", "value"));
        Assert.Equal("ヤマダタロウ", GetAttribute(document, "Kana", "value"));
        Assert.Equal("AB12", GetAttribute(document, "Code", "value"));
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
            ["Code"] = "ａｂ１２",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Equal("半角英大文字と数字で入力してください", GetMessage(document, "Code"));
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
