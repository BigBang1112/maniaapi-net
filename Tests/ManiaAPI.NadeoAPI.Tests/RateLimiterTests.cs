using Microsoft.Extensions.Time.Testing;

namespace ManiaAPI.NadeoAPI.Tests;

public class RateLimiterTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);
    private readonly FakeTimeProvider time = new();
    private readonly List<DateTimeOffset> requestTimes = [];

    [Test]
    public async Task DefaultHandlers_ShareProcessLimiter()
    {
        var first = new NadeoAPIHandler();
        var second = new NadeoAPIHandler();

        await Assert.That(first.RequestLimiter).IsSameReferenceAs(RateLimiter.Shared);
        await Assert.That(second.RequestLimiter).IsSameReferenceAs(first.RequestLimiter);
        await Assert.That(NadeoAPI.RequestInterval).IsEqualTo(Interval);
    }

    [Test]
    public async Task SendAsync_ConcurrentServices_SpaceEveryRequest()
    {
        var limiter = new RateLimiter(time);
        using var core = new NadeoServices(CreateHttpClient(), Handler(limiter));
        using var live = new NadeoLiveServices(CreateHttpClient(), Handler(limiter));
        using var meet = new NadeoMeetServices(CreateHttpClient(), Handler(limiter));

        await SendAsync(core);
        var remaining = Task.WhenAll(SendAsync(live), SendAsync(meet), SendAsync(core));
        await Assert.That(requestTimes.Count).IsEqualTo(1);
        await AdvanceUntilCompletedAsync(remaining);

        await AssertSpacedAsync(4, Interval);
    }

    [Test]
    public async Task SendAsync_SlowerInterval_SpacesSustainedRequests()
    {
        var interval = TimeSpan.FromSeconds(2);
        var limiter = new RateLimiter(time) { RequestInterval = interval };
        using var core = new NadeoServices(CreateHttpClient(), Handler(limiter));

        await SendAsync(core);
        await AdvanceUntilCompletedAsync(SendAsync(core));

        await AssertSpacedAsync(2, interval);
    }

    [Test]
    public async Task SendAsync_AfterIdle_DoesNotAccumulateBurstAllowance()
    {
        using var core = new NadeoServices(CreateHttpClient(), Handler(new RateLimiter(time)));
        await SendAsync(core);
        time.Advance(TimeSpan.FromMinutes(1));
        await SendAsync(core);

        var next = SendAsync(core);
        await Assert.That(requestTimes.Count).IsEqualTo(2);
        await AdvanceUntilCompletedAsync(next);

        await AssertSpacedAsync(3, Interval);
    }

    [Test]
    public async Task AuthorizeAndRefreshAsync_SharePacingWithServiceRequests()
    {
        var call = 0;
        var fake = new FakeHttpMessageHandler(request =>
        {
            requestTimes.Add(time.GetUtcNow());
            return ++call switch
            {
                1 => FakeResponse.Json(NadeoTokens.Response(NadeoTokens.AccessToken("NadeoServices",
                    DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(-30)))),
                2 => NadeoTokens.Valid("NadeoServices", out _),
                _ => FakeResponse.Json("[]")
            };
        });
        using var core = new NadeoServices(fake.CreateClient(), Handler(new RateLimiter(time)));

        await core.AuthorizeAsync("login", "password");
        await AdvanceUntilCompletedAsync(core.GetZonesAsync());

        await Assert.That(fake.Requests.Select(x => x.Uri.AbsolutePath)).IsEquivalentTo(
            ["/v2/authentication/token/basic", "/v2/authentication/token/refresh", "/zones/"]);
        await AssertSpacedAsync(3, Interval);
    }

    [Test]
    public async Task SendAsync_CancelledWaiters_DoNotSendOrReserveLaterSlots()
    {
        using var core = new NadeoServices(CreateHttpClient(), Handler(new RateLimiter(time)));
        await SendAsync(core);
        using var firstCancellation = new CancellationTokenSource();
        using var queuedCancellation = new CancellationTokenSource();
        var first = SendAsync(core, firstCancellation.Token);
        var queued = SendAsync(core, queuedCancellation.Token);

        await queuedCancellation.CancelAsync();
        await firstCancellation.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => first);
        await Assert.ThrowsAsync<OperationCanceledException>(() => queued);
        await Assert.That(requestTimes.Count).IsEqualTo(1);

        var next = SendAsync(core);
        time.Advance(Interval);
        await next.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(requestTimes[1] - requestTimes[0]).IsEqualTo(Interval);
    }

    [Test]
    public async Task SendAsync_NetworkFailure_StillSpacesNextAttempt()
    {
        var call = 0;
        var fake = new FakeHttpMessageHandler(_ =>
        {
            requestTimes.Add(time.GetUtcNow());
            return ++call == 1 ? throw new HttpRequestException("Connection failed") : FakeResponse.Json("[]");
        });
        using var core = new NadeoServices(fake.CreateClient(), Handler(new RateLimiter(time)));

        await Assert.ThrowsAsync<NadeoAPIResponseException>(() => SendAsync(core));
        await AdvanceUntilCompletedAsync(SendAsync(core));

        await AssertSpacedAsync(2, Interval);
    }

    [Test]
    public async Task SendAsync_SlowResponse_DoesNotBlockOtherRequestsAfterInterval()
    {
        var pendingResponse = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var http = new HttpClient(new PendingResponseHandler(pendingResponse.Task));
        var limiter = new RateLimiter(time);
        using var slow = new NadeoServices(http, Handler(limiter));
        using var other = new NadeoServices(CreateHttpClient(), Handler(limiter));
        var pending = SendAsync(slow);

        try
        {
            await AdvanceUntilCompletedAsync(SendAsync(other));
            await Assert.That(pending.IsCompleted).IsFalse();
        }
        finally
        {
            pendingResponse.SetResult(FakeResponse.Json("[]"));
            await pending;
        }
    }

    [Test]
    public async Task SendAsync_Forbidden_ThrowsWithoutRetryOrReauthorization()
    {
        var fake = FakeHttpMessageHandler.Text("Forbidden", statusCode: HttpStatusCode.Forbidden);
        using var core = new NadeoServices(fake.CreateClient(), Handler(new RateLimiter(time)));

        var error = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => SendAsync(core));

        await Assert.That(error!.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(fake.Requests.Count).IsEqualTo(1);
    }

    [Test]
    [Arguments(-1)]
    [Arguments(0)]
    [Arguments(499)]
    public void RequestInterval_FasterThanGuidance_Throws(int milliseconds)
    {
        var limiter = new RateLimiter(time);

        Assert.Throws<ArgumentOutOfRangeException>(() => limiter.RequestInterval = TimeSpan.FromMilliseconds(milliseconds));
    }

    private HttpClient CreateHttpClient() => new FakeHttpMessageHandler(_ =>
    {
        requestTimes.Add(time.GetUtcNow());
        return FakeResponse.Json("[]");
    }).CreateClient();

    private static NadeoAPIHandler Handler(RateLimiter limiter) => new() { RequestLimiter = limiter };

    private static async Task SendAsync(NadeoAPI api, CancellationToken cancellationToken = default)
    {
        using var response = await api.SendAsync(HttpMethod.Get, "test", cancellationToken: cancellationToken);
    }

    private async Task AdvanceUntilCompletedAsync(Task task)
    {
        for (var step = 0; !task.IsCompleted && step < 100; step++)
        {
            await Task.Delay(10);
            time.Advance(TimeSpan.FromMilliseconds(100));
        }

        await task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private async Task AssertSpacedAsync(int count, TimeSpan interval)
    {
        await Assert.That(requestTimes.Count).IsEqualTo(count);
        for (var index = 1; index < count; index++)
        {
            await Assert.That(requestTimes[index] - requestTimes[index - 1]).IsGreaterThanOrEqualTo(interval);
        }
    }

    private sealed class PendingResponseHandler(Task<HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => response;
    }
}
