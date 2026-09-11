namespace Template.WebApp;

using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;

// Auth:Enabled=false(既定)のとき、認証の仕組みは残したまま利用者に何も要求しないことを確認する
public sealed class AuthDisabledTests : IClassFixture<AuthDisabledApplicationFactory>
{
    private readonly AuthDisabledApplicationFactory factory;

    public AuthDisabledTests(AuthDisabledApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task RootIsAccessibleWithoutLogin()
    {
        // Arrange
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        var response = await client.GetAsync(new Uri("/", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiIsAccessibleWithoutLogin()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/data/list", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdministratorPolicyIsAlsoOpen()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act(削除はAdministratorポリシー。存在しないIDなので認可を通った証拠として404を期待する)
        var response = await client.PostAsync(new Uri("/api/data/delete/999999", UriKind.Relative), null, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LoginStillWorksWhenAuthDisabled()
    {
        // Arrange
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginResponse = await client.GetAsync(new Uri("/account/login", UriKind.Relative), TestContext.Current.CancellationToken);
        var loginDocument = await new HtmlParser().ParseDocumentAsync(await loginResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var token = loginDocument.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;

        // Act
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "admin",
            ["Password"] = "admin",
            ["__RequestVerificationToken"] = token
        });
        var response = await client.PostAsync(new Uri("/account/login", UriKind.Relative), content, TestContext.Current.CancellationToken);

        // Assert(ログイン成功はトップへのリダイレクト)
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
