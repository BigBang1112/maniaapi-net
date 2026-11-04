using System.Net.Http.Headers;
using System.Text;

namespace ManiaAPI.Testing;

/// <summary>
/// Records every request and answers it with a configured response, without touching the network.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;

    public List<RecordedRequest> Requests { get; } = [];

    public RecordedRequest Request => Requests.Count == 1
        ? Requests[0]
        : throw new InvalidOperationException($"Expected exactly one request, but {Requests.Count} were sent.");

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        this.respond = respond;
    }

    /// <summary>
    /// Answers every request with a fresh copy of the same JSON body.
    /// </summary>
    public static FakeHttpMessageHandler Json(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new FakeHttpMessageHandler(_ => FakeResponse.Json(json, statusCode));
    }

    /// <summary>
    /// Answers every request with a fresh copy of the same text body.
    /// </summary>
    public static FakeHttpMessageHandler Text(string text, string mediaType = "text/plain", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new FakeHttpMessageHandler(_ => FakeResponse.Text(text, mediaType, statusCode));
    }

    /// <summary>
    /// Answers requests with the given responses in order, and fails when more requests are sent.
    /// </summary>
    public static FakeHttpMessageHandler Sequence(params Func<HttpResponseMessage>[] responses)
    {
        var index = 0;
        return new FakeHttpMessageHandler(request => index < responses.Length
            ? responses[index++]()
            : throw new InvalidOperationException($"Unexpected request #{index + 1}: {request.Method} {request.RequestUri}"));
    }

    public HttpClient CreateClient(string? baseAddress = null)
    {
        var client = new HttpClient(this);

        if (baseAddress is not null)
        {
            client.BaseAddress = new Uri(baseAddress);
        }

        return client;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        var headers = request.Headers
            .Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
            .ToDictionary(x => x.Key, x => string.Join(", ", x.Value), StringComparer.OrdinalIgnoreCase);

        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, request.Headers.Authorization, headers, body));

        var response = respond(request);
        response.RequestMessage ??= request;
        return response;
    }
}

/// <param name="Method">HTTP method.</param>
/// <param name="Uri">Absolute request URI.</param>
/// <param name="Authorization">Authorization header, if any.</param>
/// <param name="Headers">Request and content headers, with multiple values joined by a comma.</param>
/// <param name="Body">Request body read as a string, or null when there was no content.</param>
internal sealed record RecordedRequest(
    HttpMethod Method,
    Uri Uri,
    AuthenticationHeaderValue? Authorization,
    IReadOnlyDictionary<string, string> Headers,
    string? Body)
{
    public string Url => Uri.ToString();
}

internal static class FakeResponse
{
    public static HttpResponseMessage Json(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return Text(json, "application/json", statusCode);
    }

    public static HttpResponseMessage Text(string text, string mediaType = "text/plain", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(text, Encoding.UTF8, mediaType)
        };
    }

    public static HttpResponseMessage Status(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode);
    }
}
