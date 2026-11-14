using System.Collections.Concurrent;

namespace ManiaAPI.Testing;

/// <summary>
/// Answers requests through an async callback and records their URLs safely across threads,
/// for tests that send requests at the same time. The callback can hold a response back, for example until a <see cref="TaskCompletionSource"/> is set.
/// </summary>
internal sealed class ConcurrentFakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
{
    private readonly ConcurrentQueue<string> urls = new();

    /// <summary>
    /// URLs of the requests, in the order they were sent.
    /// </summary>
    public IReadOnlyCollection<string> Urls => urls;

    /// <summary>
    /// Creates a client that doesn't dispose this handler, so several clients can share it.
    /// </summary>
    public HttpClient CreateClient() => new(this, disposeHandler: false);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        urls.Enqueue(request.RequestUri!.ToString());
        return respond(request);
    }
}
