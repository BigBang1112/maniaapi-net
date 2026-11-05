using Microsoft.Extensions.Time.Testing;

namespace ManiaAPI.TrackmaniaIO.Tests;

public class RateLimiterTests
{
    private const string UserAgent = "ManiaAPI.TrackmaniaIO.Tests";
    private const string AdsJson = """{"ads":[]}""";

    private readonly FakeTimeProvider time = new();
    private readonly List<DateTimeOffset> requestTimes = [];

    private HttpResponseMessage Respond(HttpStatusCode statusCode = HttpStatusCode.OK, long? remaining = null, long? reset = null, TimeSpan? retryAfter = null)
    {
        lock (requestTimes)
        {
            requestTimes.Add(time.GetUtcNow());
        }

        var response = FakeResponse.Json(AdsJson, statusCode);

        if (remaining.HasValue)
        {
            response.Headers.Add("X-Ratelimit-Remaining", remaining.Value.ToString());
        }

        if (reset.HasValue)
        {
            response.Headers.Add("X-Ratelimit-Reset", reset.Value.ToString());
        }

        if (retryAfter.HasValue)
        {
            response.Headers.RetryAfter = new(retryAfter.Value);
        }

        return response;
    }

    private int RequestCount
    {
        get
        {
            lock (requestTimes)
            {
                return requestTimes.Count;
            }
        }
    }

    private TrackmaniaIO CreateClient(FakeHttpMessageHandler handler)
    {
        return new TrackmaniaIO(handler.CreateClient(), UserAgent, new RateLimiter(time));
    }

    /// <summary>
    /// Advances the fake clock one second at a time until the task completes, so that every pending delay can elapse.
    /// </summary>
    private async Task<T> RunAsync<T>(Task<T> task, TimeSpan max)
    {
        for (var elapsed = TimeSpan.Zero; !task.IsCompleted && elapsed < max; elapsed += TimeSpan.FromSeconds(1))
        {
            await Task.Delay(10);
            time.Advance(TimeSpan.FromSeconds(1));
        }

        return await task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// The fake clock moves in one second steps, so a wait may show up to a second longer than requested, but never shorter.
    /// </summary>
    private static async Task AssertWaitedAsync(TimeSpan actual, TimeSpan expected)
    {
        await Assert.That(actual).IsGreaterThanOrEqualTo(expected);
        await Assert.That(actual).IsLessThanOrEqualTo(expected + TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task UsedUpLimit_WaitsForReset()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(remaining: 0, reset: 30),
            () => Respond());
        using var tmio = CreateClient(handler);

        await tmio.GetAdsAsync();
        var second = tmio.GetAdsAsync();

        await Task.Delay(50);
        await Assert.That(RequestCount).IsEqualTo(1);

        await RunAsync(second, TimeSpan.FromMinutes(1));

        await AssertWaitedAsync(requestTimes[1] - requestTimes[0], TimeSpan.FromSeconds(30));
    }

    [Test]
    public async Task TooManyRequests_RetriesAfterRetryAfter()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests, retryAfter: TimeSpan.FromSeconds(5)),
            () => Respond());
        using var tmio = CreateClient(handler);

        var ads = await RunAsync(tmio.GetAdsAsync(), TimeSpan.FromMinutes(1));

        await Assert.That(ads.Ads).IsEmpty();
        await AssertWaitedAsync(requestTimes[1] - requestTimes[0], TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task TooManyRequests_WithoutRetryAfter_WaitsForReset()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests, remaining: 0, reset: 12),
            () => Respond());
        using var tmio = CreateClient(handler);

        await RunAsync(tmio.GetAdsAsync(), TimeSpan.FromMinutes(1));

        await AssertWaitedAsync(requestTimes[1] - requestTimes[0], TimeSpan.FromSeconds(12));
    }

    [Test]
    public async Task TooManyRequests_WithoutHeaders_WaitsDefaultDelay()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests),
            () => Respond());
        using var tmio = CreateClient(handler);

        await RunAsync(tmio.GetAdsAsync(), TimeSpan.FromMinutes(1));

        await AssertWaitedAsync(requestTimes[1] - requestTimes[0], RateLimiter.DefaultRetryDelay);
    }

    [Test]
    public async Task TooManyRequests_AfterMaxRetries_Throws()
    {
        var handler = new FakeHttpMessageHandler(_ => Respond(HttpStatusCode.TooManyRequests, retryAfter: TimeSpan.FromSeconds(1)));
        using var tmio = CreateClient(handler);
        tmio.MaxRateLimitRetries = 2;

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => RunAsync(tmio.GetAdsAsync(), TimeSpan.FromMinutes(1)));

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.TooManyRequests);
        await Assert.That(RequestCount).IsEqualTo(3);
    }

    [Test]
    public async Task TooManyRequests_NoRetries_ThrowsImmediately()
    {
        var handler = new FakeHttpMessageHandler(_ => Respond(HttpStatusCode.TooManyRequests, retryAfter: TimeSpan.FromSeconds(1)));
        using var tmio = CreateClient(handler);
        tmio.MaxRateLimitRetries = 0;

        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());

        await Assert.That(RequestCount).IsEqualTo(1);
    }

    [Test]
    public async Task TooManyRequests_HoldsBackOtherRequests()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests, retryAfter: TimeSpan.FromSeconds(20)),
            () => Respond());
        using var tmio = CreateClient(handler);
        tmio.MaxRateLimitRetries = 0;

        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());
        var next = tmio.GetAdsAsync();

        await Task.Delay(50);
        await Assert.That(RequestCount).IsEqualTo(1);

        await RunAsync(next, TimeSpan.FromMinutes(1));

        await AssertWaitedAsync(requestTimes[1] - requestTimes[0], TimeSpan.FromSeconds(20));
    }

    [Test]
    public async Task ErrorResponse_StillUpdatesLimit()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.InternalServerError, remaining: 0, reset: 15),
            () => Respond());
        using var tmio = CreateClient(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());
        await RunAsync(tmio.GetAdsAsync(), TimeSpan.FromMinutes(1));

        await AssertWaitedAsync(requestTimes[1] - requestTimes[0], TimeSpan.FromSeconds(15));
    }

    [Test]
    public async Task ParallelRequests_DoNotExceedRemaining()
    {
        var first = true;
        var handler = new FakeHttpMessageHandler(_ =>
        {
            if (first)
            {
                first = false;
                return Respond(remaining: 2, reset: 60);
            }

            return Respond();
        });
        using var tmio = CreateClient(handler);

        await tmio.GetAdsAsync();
        var parallel = Enumerable.Range(0, 5).Select(_ => tmio.GetAdsAsync()).ToArray();

        await Task.Delay(50);
        await Assert.That(RequestCount).IsEqualTo(3);

        await RunAsync(Task.WhenAll(parallel), TimeSpan.FromMinutes(2));

        await Assert.That(RequestCount).IsEqualTo(6);
        await Assert.That(requestTimes.Skip(3)).DoesNotContain(x => x - requestTimes[0] < TimeSpan.FromSeconds(60));
    }

    [Test]
    public async Task ParallelRequests_ServerRemainingExcludesRequestsInFlight()
    {
        var limiter = new RateLimiter(time);

        await limiter.AcquireAsync(CancellationToken.None);
        await limiter.AcquireAsync(CancellationToken.None);

        // The first of two requests in flight comes back saying one request is left, which the second one will use.
        limiter.Complete(Respond(remaining: 1, reset: 60));

        var third = limiter.AcquireAsync(CancellationToken.None);

        await Task.Delay(50);
        await Assert.That(third.IsCompleted).IsFalse();

        time.Advance(TimeSpan.FromSeconds(60));
        await third.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task FailedRequest_ReleasesReservation()
    {
        var call = 0;
        var handler = new FakeHttpMessageHandler(_ => ++call switch
        {
            1 => Respond(remaining: 1, reset: 60),
            2 => throw new HttpRequestException("Connection refused."),
            _ => Respond()
        });
        using var tmio = CreateClient(handler);

        await tmio.GetAdsAsync();
        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());

        // The failed request never reached the server, so the last request of the window is still available without waiting.
        await tmio.GetAdsAsync().WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(call).IsEqualTo(3);
    }

    [Test]
    public async Task Cancellation_WhileWaiting_Throws()
    {
        var handler = FakeHttpMessageHandler.Sequence(() => Respond(remaining: 0, reset: 60));
        using var tmio = CreateClient(handler);
        using var cts = new CancellationTokenSource();

        await tmio.GetAdsAsync();
        var waiting = tmio.GetAdsAsync(cts.Token);
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => waiting);
    }

    [Test]
    public async Task PublicConstructors_ShareOneRateLimiter()
    {
        using var a = new TrackmaniaIO(new HttpClient(), UserAgent);
        using var b = new TrackmaniaIO(UserAgent);

        await Assert.That(GetRateLimiter(a)).IsSameReferenceAs(RateLimiter.Shared);
        await Assert.That(GetRateLimiter(b)).IsSameReferenceAs(RateLimiter.Shared);
    }

    private static RateLimiter? GetRateLimiter(TrackmaniaIO tmio)
    {
        return (RateLimiter?)typeof(TrackmaniaIO)
            .GetField("rateLimiter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(tmio);
    }
}
