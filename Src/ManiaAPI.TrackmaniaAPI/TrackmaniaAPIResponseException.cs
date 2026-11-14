using System.Net;

namespace ManiaAPI.TrackmaniaAPI;

/// <summary>
/// Thrown when the Trackmania web API answers with an error status, or when the request failed without a response.
/// <see cref="HttpRequestException.StatusCode"/> is <see langword="null"/> when no response arrived, like when the connection failed,
/// and <see cref="Exception.InnerException"/> is then the original <see cref="HttpRequestException"/>.
/// </summary>
public class TrackmaniaAPIResponseException : HttpRequestException
{
    /// <summary>
    /// Error from the response body, or <see langword="null"/> if it had none.
    /// </summary>
    public ErrorResponse? Response { get; }

    /// <summary>
    /// Reason phrase of the error response, or <see langword="null"/> if no response arrived.
    /// </summary>
    public string? ReasonPhrase { get; }

    public TrackmaniaAPIResponseException(ErrorResponse? response, HttpStatusCode statusCode, string? reasonPhrase)
        : base(CreateMessage(response, statusCode, reasonPhrase), inner: null, statusCode)
    {
        Response = response;
        ReasonPhrase = reasonPhrase;
    }

    public TrackmaniaAPIResponseException(string? message) : base(message) { }

    /// <summary>
    /// Wraps a failed request. The status code and error kind of an inner <see cref="HttpRequestException"/> are kept.
    /// </summary>
    public TrackmaniaAPIResponseException(string? message, Exception? innerException)
        : base((innerException as HttpRequestException)?.HttpRequestError ?? HttpRequestError.Unknown,
            message, innerException, (innerException as HttpRequestException)?.StatusCode) { }

    private static string CreateMessage(ErrorResponse? response, HttpStatusCode statusCode, string? reasonPhrase)
    {
        if (!string.IsNullOrEmpty(response?.Message))
        {
            return response.Message;
        }

        return string.IsNullOrEmpty(reasonPhrase)
            ? $"Response status code does not indicate success: {(int)statusCode}."
            : reasonPhrase;
    }
}
