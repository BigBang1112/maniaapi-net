namespace ManiaAPI.NadeoAPI;

/// <summary>
/// Spaces request starts across all Nadeo services, including authentication and token refreshes.
/// </summary>
internal sealed class RateLimiter(TimeProvider timeProvider)
{
    public static RateLimiter Shared { get; } = new(TimeProvider.System);

    private readonly SemaphoreSlim dispatchLock = new(1, 1);
    private long requestIntervalTicks = TimeSpan.FromMilliseconds(500).Ticks;
    private long? lastRequestTimestamp;

    public TimeSpan RequestInterval
    {
        get => TimeSpan.FromTicks(Interlocked.Read(ref requestIntervalTicks));
        set
        {
            if (value < TimeSpan.FromMilliseconds(500))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Nadeo requests must be spaced at least 500 milliseconds apart.");
            }

            Interlocked.Exchange(ref requestIntervalTicks, value.Ticks);
        }
    }

    public async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await dispatchLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        Task<HttpResponseMessage> response;

        try
        {
            while (lastRequestTimestamp is { } last)
            {
                var delay = RequestInterval - timeProvider.GetElapsedTime(last);
                if (delay <= TimeSpan.Zero)
                {
                    break;
                }

                // Bound each delay so even very long configured intervals fit the timer's range.
                await Task.Delay(delay > TimeSpan.FromDays(1) ? TimeSpan.FromDays(1) : delay, timeProvider, cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            lastRequestTimestamp = timeProvider.GetTimestamp();
            // Start the request before releasing the lock, but don't hold the lock while awaiting its response.
            response = client.SendAsync(request, cancellationToken);
        }
        finally
        {
            dispatchLock.Release();
        }

        return await response.ConfigureAwait(false);
    }
}
