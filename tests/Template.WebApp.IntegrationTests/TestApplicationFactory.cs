namespace Template.WebApp;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseFile = $"test-{Guid.NewGuid():N}.db";

    // 既存のテストは認証が効いている状態を前提にする。既定(appsettings)はオフのため明示する
    protected virtual bool AuthEnabled => true;

    public Task<HttpClient> CreateAuthenticatedClientAsync() =>
        CreateAuthenticatedClientAsync(new WebApplicationFactoryClientOptions());

    public async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplicationFactoryClientOptions options)
    {
        var client = CreateClient(options);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "admin",
            ["Password"] = "admin"
        });
        await client.PostAsync(new Uri("/account/login", UriKind.Relative), form, TestContext.Current.CancellationToken);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("http_ports", string.Empty);
        builder.UseSetting("Auth:Enabled", AuthEnabled ? "true" : "false");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={databaseFile};Cache=Shared;Pooling=False");
        builder.UseSetting("Prometheus:Uri", string.Empty);
        builder.UseSetting("Profiler:SqlLog:Enable", "false");
        builder.UseSetting("Profiler:SqlTelemetry:Enable", "false");
        builder.UseSetting("Log:HttpLog", "false");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(databaseFile))
        {
            try
            {
                File.Delete(databaseFile);
            }
            catch (IOException)
            {
                // Ignore
            }
        }
    }
}
