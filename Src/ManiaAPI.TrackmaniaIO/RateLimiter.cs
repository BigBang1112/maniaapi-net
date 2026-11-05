using System.Net;

namespace ManiaAPI.TrackmaniaIO;

/// <summary>
/// Tracks the trackmania.io rate limit from the <c>X-Ratelimit-*</c> response headers and holds requests back once the limit is used up.
/// </summary>
/// <remarks>
/// The limit belongs to the client as a whole, not to one <see cref="TrackmaniaIO"/> instance, so <see cref="Shared"/> is used by default.
/// </remarks>
internal sealed class RateLimiter
{
    private const string RemainingHeader = "X-Ratelimit-Remaining";
    private const string ResetHeader = "X-Ratelimit-Reset";

    /// <summary>
    /// How long to wait after a 429 response that doesn't say how long to wait.
    /// </summary>
    internal static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan MinRetryDelay = TimeSpan.FromSeconds(1);

    public static RateLimiter Shared { get; } = new(TimeProvider.System);

    private readonly TimeProvider timeProvider;
    private readonly object sync = new();

    /// <summary>
    /// Requests left in the current window, already reduced by the requests in flight. Null when unknown.
    /// </summary>
    private long? remaining;
    private DateTimeOffset? reset;
    private int inFlight;

    public RateLimiter(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    /// <summary>
    /// Waits until a request may be sent and reserves it. Every successful call must be followed by <see cref="Complete"/> or <see cref="Cancel"/>.
    /// </summary>
    public async Task AcquireAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            TimeSpan delay;

            lock (sync)
            {
                var now = timeProvider.GetUtcNow();

                if (reset <= now)
                {
                    // The window is over, the next response will tell the new state.
                    remaining = null;
                    reset = null;
                }

                if (remaining is null or > 0 || reset is null)
                {
                    remaining--;
                    inFlight++;
                    return;
                }

                delay = reset.Value - now;
            }

            await Task.Delay(delay, timeProvider, cancellationToken);
        }
    }

    /// <summary>
    /// Releases a reservation for a request that got no response.
    /// </summary>
    public void Cancel()
    {
        lock (sync)
        {
            inFlight--;
            remaining++;
        }
    }

    /// <summary>
    /// Releases a reservation and updates the state from the response headers.
    /// </summary>
    /// <returns>For a 429 response, how long to wait before retrying; otherwise null.</returns>
    public TimeSpan? Complete(HttpResponseMessage response)
    {
        var serverRemaining = GetHeaderNumberValue(RemainingHeader, response);
        var resetSeconds = GetHeaderNumberValue(ResetHeader, response);

        lock (sync)
        {
            inFlight--;

            var now = timeProvider.GetUtcNow();

            if (resetSeconds.HasValue)
            {
                reset = now + TimeSpan.FromSeconds(resetSeconds.Value);
            }

            if (serverRemaining.HasValue)
            {
                // The server hasn't counted the other requests in flight yet.
                remaining = Math.Max(serverRemaining.Value - inFlight, 0);
            }

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
            {
                return null;
            }

            var delay = response.Headers.RetryAfter?.Delta
                ?? (response.Headers.RetryAfter?.Date - now)
                ?? (resetSeconds.HasValue ? TimeSpan.FromSeconds(resetSeconds.Value) : DefaultRetryDelay);

            if (delay < MinRetryDelay)
            {
                delay = MinRetryDelay;
            }

            // Hold back every request, not only the one that will be retried.
            remaining = 0;
            reset = now + delay;

            return delay;
        }
    }

    internal static long? GetHeaderNumberValue(string name, HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(name, out IEnumerable<string>? values))
        {
            return null;
        }

        return long.TryParse(values.FirstOrDefault(), out long value) ? value : null;
    }
}
