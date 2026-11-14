using Microsoft.Extensions.Time.Testing;

namespace ManiaAPI.TrackmaniaIO.Tests;

/// <summary>
/// Every request goes through the same rate limiting, so <see cref="TrackmaniaIO.GetAdsAsync"/> stands in for all of them.
/// </summary>
public class RateLimiterTests
{
    private const string UserAgent = "ManiaAPI.TrackmaniaIO.Tests";
    private const string EmptyAdsJson = """{"ads":[]}""";
    private const string RemainingHeader = "X-Ratelimit-Remaining";
    private const string ResetHeader = "X-Ratelimit-Reset";

    private static readonly TimeSpan Reset = TimeSpan.FromSeconds(30);

    private readonly FakeTimeProvider time = new();
    private readonly List<DateTimeOffset> requestTimes = [];

    [Test]
    public async Task GetAdsAsync_LimitUsedUp_WaitsForReset()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(remaining: 0, reset: Reset),
            () => Respond());
        using var tmio = CreateClient(handler);
        await tmio.GetAdsAsync();

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), Reset);
    }

    [Test]
    public async Task GetAdsAsync_LimitNotUsedUp_DoesNotWait()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(remaining: 1, reset: Reset),
            () => Respond());
        using var tmio = CreateClient(handler);
        await tmio.GetAdsAsync();

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), TimeSpan.Zero);
    }

    [Test]
    public async Task GetAdsAsync_TooManyRequestsWithRetryAfter_RetriesAfterRetryAfter()
    {
        var retryAfter = TimeSpan.FromSeconds(5);
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests, retryAfter: retryAfter),
            () => Respond());
        using var tmio = CreateClient(handler);

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), retryAfter);
    }

    [Test]
    public async Task GetAdsAsync_TooManyRequestsWithResetHeader_RetriesAfterReset()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests, remaining: 0, reset: Reset),
            () => Respond());
        using var tmio = CreateClient(handler);

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), Reset);
    }

    [Test]
    public async Task GetAdsAsync_TooManyRequestsWithoutHeaders_RetriesAfterDefaultDelay()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests),
            () => Respond());
        using var tmio = CreateClient(handler);

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), RateLimiter.DefaultRetryDelay);
    }

    [Test]
    public async Task GetAdsAsync_TooManyRequestsThenSuccess_ReturnsRetriedResponse()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests, retryAfter: TimeSpan.FromSeconds(1)),
            () => Respond(json: """{"ads":[{"name":"Retried"}]}"""));
        using var tmio = CreateClient(handler);

        var ads = await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await Assert.That(ads.Ads.Select(x => x.Name)).IsEquivalentTo(["Retried"]);
    }

    [Test]
    public async Task GetAdsAsync_TooManyRequestsBeyondMaxRetries_ThrowsTooManyRequests()
    {
        const int maxRetries = 2;
        var handler = new FakeHttpMessageHandler(_ => Respond(HttpStatusCode.TooManyRequests, retryAfter: TimeSpan.FromSeconds(1)));
        using var tmio = CreateClient(handler);
        tmio.MaxRateLimitRetries = maxRetries;

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => AdvanceUntilCompletedAsync(tmio.GetAdsAsync()));

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.TooManyRequests);
        await Assert.That(requestTimes.Count).IsEqualTo(maxRetries + 1);
    }

    [Test]
    public async Task GetAdsAsync_TooManyRequestsWithRetriesDisabled_ThrowsWithoutRetrying()
    {
        var handler = new FakeHttpMessageHandler(_ => Respond(HttpStatusCode.TooManyRequests, retryAfter: TimeSpan.FromSeconds(1)));
        using var tmio = CreateClient(handler);
        tmio.MaxRateLimitRetries = 0;

        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());

        await Assert.That(requestTimes.Count).IsEqualTo(1);
    }

    [Test]
    public async Task GetAdsAsync_AfterTooManyRequests_HoldsBackNextRequest()
    {
        var retryAfter = TimeSpan.FromSeconds(20);
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.TooManyRequests, retryAfter: retryAfter),
            () => Respond());
        using var tmio = CreateClient(handler);
        tmio.MaxRateLimitRetries = 0;
        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), retryAfter);
    }

    [Test]
    public async Task GetAdsAsync_ErrorResponseWithLimitHeaders_WaitsForReset()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            () => Respond(HttpStatusCode.InternalServerError, remaining: 0, reset: Reset),
            () => Respond());
        using var tmio = CreateClient(handler);
        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), Reset);
    }

    [Test]
    public async Task GetAdsAsync_ParallelRequests_SendsOnlyRemainingBeforeReset()
    {
        const int remaining = 2;
        const int parallelRequests = 5;
        var isFirst = true;
        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = isFirst ? Respond(remaining: remaining, reset: Reset) : Respond();
            isFirst = false;
            return response;
        });
        using var tmio = CreateClient(handler);
        await tmio.GetAdsAsync();

        await AdvanceUntilCompletedAsync(Task.WhenAll(Enumerable.Range(0, parallelRequests).Select(_ => tmio.GetAdsAsync())));

        var sentBeforeReset = requestTimes.Skip(1).Count(x => x - requestTimes[0] < Reset);
        await Assert.That(sentBeforeReset).IsEqualTo(remaining);
    }

    [Test]
    public async Task GetAdsAsync_RequestFailsWithoutResponse_DoesNotUseUpLimit()
    {
        var call = 0;
        var handler = new FakeHttpMessageHandler(_ => ++call switch
        {
            1 => Respond(remaining: 1, reset: Reset),
            2 => throw new HttpRequestException("Connection refused."),
            _ => Respond()
        });
        using var tmio = CreateClient(handler);
        await tmio.GetAdsAsync();
        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetAdsAsync());

        await AdvanceUntilCompletedAsync(tmio.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), TimeSpan.Zero);
    }

    [Test]
    public async Task GetAdsAsync_CancelledWhileWaitingForReset_ThrowsTaskCanceled()
    {
        var handler = FakeHttpMessageHandler.Sequence(() => Respond(remaining: 0, reset: Reset));
        using var tmio = CreateClient(handler);
        using var cts = new CancellationTokenSource();
        await tmio.GetAdsAsync();
        var waiting = tmio.GetAdsAsync(cts.Token);

        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() => waiting);
    }

    [Test]
    public async Task GetAdsAsync_ClientsSharingRateLimiter_ShareLimit()
    {
        var limiter = new RateLimiter(time);
        using var first = CreateClient(FakeHttpMessageHandler.Sequence(() => Respond(remaining: 0, reset: Reset)), limiter);
        using var second = CreateClient(FakeHttpMessageHandler.Sequence(() => Respond()), limiter);
        await first.GetAdsAsync();

        await AdvanceUntilCompletedAsync(second.GetAdsAsync());

        await AssertWaitedAsync(WaitBeforeRequest(1), Reset);
    }

    [Test]
    public async Task AcquireAsync_ServerRemainingWithRequestsInFlight_CountsThemAsUsed()
    {
        var limiter = new RateLimiter(time);
        await limiter.AcquireAsync(CancellationToken.None);
        await limiter.AcquireAsync(CancellationToken.None);
        // One request left according to the server, but the second request in flight will use it.
        limiter.Complete(CreateResponse(HttpStatusCode.OK, EmptyAdsJson, remaining: 1, reset: Reset, retryAfter: null));
        var start = time.GetUtcNow();

        await AdvanceUntilCompletedAsync(limiter.AcquireAsync(CancellationToken.None));

        await AssertWaitedAsync(time.GetUtcNow() - start, Reset);
    }

    [Test]
    public async Task GetHeaderNumberValue_NumericHeader_ReturnsValue()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Add(RemainingHeader, "123");

        var value = RateLimiter.GetHeaderNumberValue(RemainingHeader, response);

        await Assert.That(value).IsEqualTo(123);
    }

    [Test]
    public async Task GetHeaderNumberValue_MissingHeader_ReturnsNull()
    {
        using var response = new HttpResponseMessage();

        var value = RateLimiter.GetHeaderNumberValue(RemainingHeader, response);

        await Assert.That(value).IsNull();
    }

    [Test]
    public async Task GetHeaderNumberValue_NonNumericHeader_ReturnsNull()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Add(RemainingHeader, "value");

        var value = RateLimiter.GetHeaderNumberValue(RemainingHeader, response);

        await Assert.That(value).IsNull();
    }

    private TrackmaniaIO CreateClient(FakeHttpMessageHandler handler, RateLimiter? limiter = null)
    {
        return new TrackmaniaIO(handler.CreateClient(), UserAgent, limiter ?? new RateLimiter(time));
    }

    /// <summary>
    /// Records when the request arrived and answers it.
    /// </summary>
    private HttpResponseMessage Respond(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        long? remaining = null,
        TimeSpan? reset = null,
        TimeSpan? retryAfter = null,
        string json = EmptyAdsJson)
    {
        lock (requestTimes)
        {
            requestTimes.Add(time.GetUtcNow());
        }

        return CreateResponse(statusCode, json, remaining, reset, retryAfter);
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string json, long? remaining, TimeSpan? reset, TimeSpan? retryAfter)
    {
        var response = FakeResponse.Json(json, statusCode);

        if (remaining.HasValue)
        {
            response.Headers.Add(RemainingHeader, remaining.Value.ToString());
        }

        if (reset.HasValue)
        {
            response.Headers.Add(ResetHeader, ((long)reset.Value.TotalSeconds).ToString());
        }

        if (retryAfter.HasValue)
        {
            response.Headers.RetryAfter = new(retryAfter.Value);
        }

        return response;
    }

    private TimeSpan WaitBeforeRequest(int index)
    {
        return requestTimes[index] - requestTimes[index - 1];
    }

    /// <summary>
    /// Moves the fake clock forward one second at a time until the task completes, so that any wait can elapse.
    /// The short real delay gives continuations a chance to run between steps.
    /// </summary>
    private async Task AdvanceUntilCompletedAsync(Task task)
    {
        var maxWait = TimeSpan.FromMinutes(5);

        for (var elapsed = TimeSpan.Zero; !task.IsCompleted && elapsed < maxWait; elapsed += TimeSpan.FromSeconds(1))
        {
            await Task.Delay(10);
            time.Advance(TimeSpan.FromSeconds(1));
        }

        await task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private async Task<T> AdvanceUntilCompletedAsync<T>(Task<T> task)
    {
        await AdvanceUntilCompletedAsync((Task)task);
        return await task;
    }

    /// <summary>
    /// The fake clock moves in one second steps, so a wait can show up to a second longer than requested, but never shorter.
    /// </summary>
    private static async Task AssertWaitedAsync(TimeSpan actual, TimeSpan expected)
    {
        await Assert.That(actual).IsGreaterThanOrEqualTo(expected);
        await Assert.That(actual).IsLessThanOrEqualTo(expected + TimeSpan.FromSeconds(1));
    }
}
