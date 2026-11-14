using System.Diagnostics;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting.Tests;

public class NadeoAPIRefreshBackgroundServiceTests
{
    private const string NadeoServicesAudience = "NadeoServices";
    private const string NadeoLiveServicesAudience = "NadeoLiveServices";

    [Test]
    public async Task RefreshFails_KeepsHostRunning()
    {
        await using var host = new RefreshHost();
        host.Server.RespondToRefresh = FailRefresh;
        await host.AuthorizeAsync<NadeoServices>();
        await host.StartAsync();

        // A second attempt only happens if the first failure didn't end the service
        await host.AdvanceUntilAsync(() => host.Server.Refreshes.Count >= 2);

        await Assert.That(host.IsStopping).IsFalse();
    }

    [Test]
    public async Task RefreshFails_LogsWarningWithException()
    {
        await using var host = new RefreshHost();
        host.Server.RespondToRefresh = FailRefresh;
        await host.AuthorizeAsync<NadeoServices>();
        await host.StartAsync();

        await host.AdvanceUntilAsync(() => host.RefreshWarnings.Any());

        var warning = host.RefreshWarnings.First();
        await Assert.That(warning.Message).Contains(nameof(NadeoServices));
        await Assert.That(warning.Exception).IsTypeOf<NadeoAPIResponseException>();
    }

    [Test]
    public async Task OneServiceRefreshFails_RefreshesOtherServices()
    {
        await using var host = new RefreshHost();
        host.Server.RespondToRefresh = (audience, cancellationToken) => audience == NadeoServicesAudience
            ? FailRefresh(audience, cancellationToken)
            : Task.FromResult(FakeNadeoAuthServer.ValidToken(audience));
        await host.AuthorizeAsync<NadeoServices>();
        await host.AuthorizeAsync<NadeoLiveServices>();
        await host.StartAsync();

        await host.AdvanceUntilAsync(() => host.Server.Refreshes.Contains(NadeoLiveServicesAudience));

        var expirationTime = host.Get<NadeoLiveServices>().ExpirationTime ?? DateTimeOffset.MinValue;
        await Assert.That(expirationTime).IsGreaterThan(DateTimeOffset.UtcNow);
    }

    [Test]
    public async Task StopAsync_DuringRefresh_LogsNoWarning()
    {
        await using var host = await StartWithHangingRefreshAsync();

        await host.StopAsync();

        await Assert.That(host.RefreshWarnings).IsEmpty();
    }

    [Test]
    public async Task StopAsync_DuringRefresh_EndsWithoutFault()
    {
        await using var host = await StartWithHangingRefreshAsync();

        await host.StopAsync();

        await Assert.That(host.RefreshTask.IsFaulted).IsFalse();
    }

    private static Task<HttpResponseMessage> FailRefresh(string audience, CancellationToken cancellationToken)
    {
        return Task.FromResult(FakeResponse.Status(HttpStatusCode.InternalServerError));
    }

    /// <summary>
    /// Starts a host whose refresh request never gets a response, and waits until that request is sent.
    /// </summary>
    private static async Task<RefreshHost> StartWithHangingRefreshAsync()
    {
        var host = new RefreshHost();
        host.Server.RespondToRefresh = async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new UnreachableException();
        };
        await host.AuthorizeAsync<NadeoServices>();
        await host.StartAsync();

        await host.AdvanceUntilAsync(() => host.Server.Refreshes.Count >= 1);

        return host;
    }
}
