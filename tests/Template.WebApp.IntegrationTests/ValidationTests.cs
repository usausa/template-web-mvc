namespace Template.WebApp;

using System.Net.Http.Json;
using System.Text;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Mvc;

public sealed class ValidationTests : IClassFixture<AuthDisabledApplicationFactory>
{
    private static readonly Uri ValidationUri = new("/sample/validation", UriKind.Relative);

    private readonly AuthDisabledApplicationFactory factory;

    public ValidationTests(AuthDisabledApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task PageRendersClientRules()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var document = await GetAsync(client);
        var name = document.QuerySelector("input#Name")!;
        var code = document.QuerySelector("input#Code")!;
        var mail = document.QuerySelector("input#Mail")!;

        // Assert
        Assert.Equal("20バイト以内で入力してください", name.GetAttribute("data-val-ms932length"));
        Assert.Equal("0", name.GetAttribute("data-val-ms932length-min"));
        Assert.Equal("20", name.GetAttribute("data-val-ms932length-max"));
        Assert.Equal("全角で入力してください", name.GetAttribute("data-val-charactertype"));
        Assert.Equal("半角英大文字と数字で入力してください", code.GetAttribute("data-val-charactertype"));
        Assert.Equal("[A-Z0-9]*", code.GetAttribute("data-val-charactertype-pattern"));
        Assert.Equal("メールアドレスの形式で入力してください", mail.GetAttribute("data-val-email"));
    }

    [Fact]
    public async Task InvalidPropertiesShowMessages()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = GetToken(await GetAsync(client));

        // Act
        var document = await PostAsync(client, new Dictionary<string, string>
        {
            ["Name"] = "abc",
            ["Code"] = "ab",
            ["Phone"] = "03-1234-5678",
            ["UsePeriod"] = "true",
            ["From"] = "2026-02-01",
            ["To"] = "2026-01-01",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Equal("全角で入力してください", GetMessage(document, "Name"));
        Assert.Equal("半角英大文字と数字で入力してください", GetMessage(document, "Code"));
        Assert.Equal("大小関係が正しくありません", GetMessage(document, "To"));
    }

    [Fact]
    public async Task RequiredWhenShowsMessages()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = GetToken(await GetAsync(client));

        // Act
        var document = await PostAsync(client, new Dictionary<string, string>
        {
            ["Phone"] = "03-1234-5678",
            ["UsePeriod"] = "true",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Equal("入力してください", GetMessage(document, "From"));
        Assert.Equal("入力してください", GetMessage(document, "To"));
    }

    [Fact]
    public async Task AnyRequiredShowsMessages()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = GetToken(await GetAsync(client));

        // Act
        var document = await PostAsync(client, new Dictionary<string, string>
        {
            ["Name"] = "山田",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Equal("いずれかを入力してください", GetMessage(document, "Phone"));
        Assert.Equal("いずれかを入力してください", GetMessage(document, "Mail"));
    }

    [Fact]
    public async Task ValidInputShowsCompletion()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = GetToken(await GetAsync(client));

        // Act
        var document = await PostAsync(client, new Dictionary<string, string>
        {
            ["Name"] = "山田",
            ["Code"] = "A1",
            ["Mail"] = "user@example.com",
            ["UsePeriod"] = "true",
            ["From"] = "2026-01-01",
            ["To"] = "2026-01-31",
            ["__RequestVerificationToken"] = token
        });

        // Assert
        Assert.Contains("入力内容は正しいです", document.Body!.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApiReturnsJapaneseMessages()
    {
        // Arrange
        var client = factory.CreateClient();
        using var content = new StringContent("""{"name":"","value":1}""", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(new Uri("/api/data/create", UriKind.Relative), content, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("入力してください", Assert.Single(Assert.Single(problem.Errors).Value));
    }

    [Fact]
    public async Task ApiJsonErrorReturnsJapaneseMessage()
    {
        // Arrange
        var client = factory.CreateClient();
        using var content = new StringContent("""{"name":"JsonItem","value":"1"}""", Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync(new Uri("/api/data/create", UriKind.Relative), content, TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("形式が不正です", Assert.Single(problem.Errors["$.value"]));
    }

    [Fact]
    public async Task ApiQueryErrorReturnsJapaneseMessage()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/data/list?page=abc", UriKind.Relative), TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("形式が不正です", Assert.Single(problem.Errors["page"]));
    }

    private static async Task<IHtmlDocument> GetAsync(HttpClient client)
    {
        var response = await client.GetAsync(ValidationUri, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static async Task<IHtmlDocument> PostAsync(HttpClient client, Dictionary<string, string> values)
    {
        using var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync(ValidationUri, content, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private static string GetToken(IHtmlDocument document) =>
        document.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;

    private static string? GetMessage(IHtmlDocument document, string name) =>
        document.QuerySelector($"span[data-valmsg-for='{name}']")?.TextContent;
}
