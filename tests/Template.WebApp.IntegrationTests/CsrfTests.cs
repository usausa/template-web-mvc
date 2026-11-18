namespace Template.WebApp;

using Microsoft.AspNetCore.Mvc.Testing;

public sealed class CsrfTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public CsrfTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.Redirect)]
    public async Task LoginAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "admin",
            ["Password"] = "admin"
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/account/login", UriKind.Relative));
        request.Content = form;
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.Redirect)]
    public async Task LogoutAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/account/logout", UriKind.Relative));
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.Redirect)]
    public async Task DataCreateAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = $"Csrf-{site}",
            ["Value"] = "1"
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/data/create", UriKind.Relative));
        request.Content = form;
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.NotFound)]
    public async Task ApiDeleteAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/data/delete/999999", UriKind.Relative));
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task ApiListAllowsCrossSite()
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/data/list", UriKind.Relative));
        request.Headers.Add("Sec-Fetch-Site", "cross-site");

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.OK)]
    public async Task PasskeyCreationOptionsAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = await factory.CreateAuthenticatedClientAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/account/passkey/creation-options", UriKind.Relative));
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("cross-site", HttpStatusCode.BadRequest)]
    [InlineData("same-origin", HttpStatusCode.OK)]
    public async Task PasskeyRequestOptionsAllowsOnlySameOrigin(string site, HttpStatusCode expected)
    {
        // Arrange
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/account/passkey/request-options", UriKind.Relative));
        request.Headers.Add("Sec-Fetch-Site", site);

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, response.StatusCode);
    }
}
