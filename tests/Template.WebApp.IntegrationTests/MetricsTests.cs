namespace Template.WebApp;

using System.Diagnostics.Metrics;

using Template.WebApp.Host.Application.Telemetry;

public sealed class MetricsTests : IClassFixture<AuthDisabledApplicationFactory>
{
    private readonly AuthDisabledApplicationFactory factory;

    private long count;

    public MetricsTests(AuthDisabledApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task ApiRequestIsCounted()
    {
        // Arrange
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if ((instrument.Meter.Name == Source.Name) && (instrument.Name == "api.request.execution"))
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, measurement, _, _) => Interlocked.Add(ref count, measurement));
        listener.Start();

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/data/list", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(Interlocked.Read(ref count) >= 1);
    }
}
