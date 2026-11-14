using System.Net;

namespace ManiaAPI.NadeoAPI;

/// <summary>
/// Thrown when NadeoAPI answers with an error status, or when the request failed without a response.
/// <see cref="HttpRequestException.StatusCode"/> is <see langword="null"/> when no response arrived, like when the connection failed,
/// and <see cref="Exception.InnerException"/> is then the original <see cref="HttpRequestException"/>.
/// </summary>
public class NadeoAPIResponseException : HttpRequestException
{
    /// <summary>
    /// Error from the response body, or <see langword="null"/> if it had none.
    /// </summary>
    public ErrorResponse? Response { get; }

    /// <summary>
    /// Reason phrase of the error response, or <see langword="null"/> if no response arrived.
    /// </summary>
    public string? ReasonPhrase { get; }

    public NadeoAPIResponseException(ErrorResponse? response, HttpStatusCode statusCode, string? reasonPhrase)
        : this(response, statusCode, reasonPhrase, innerException: null) { }

    /// <summary>
    /// Creates the exception for an error response, keeping an earlier exception for the same error, like a rejection of the same credentials.
    /// </summary>
    public NadeoAPIResponseException(ErrorResponse? response, HttpStatusCode statusCode, string? reasonPhrase, Exception? innerException)
        : base(CreateMessage(response, statusCode, reasonPhrase), innerException, statusCode)
    {
        Response = response;
        ReasonPhrase = reasonPhrase;
    }

    public NadeoAPIResponseException(string? message) : base(message) { }

    /// <summary>
    /// Wraps a failed request. The status code and error kind of an inner <see cref="HttpRequestException"/> are kept.
    /// </summary>
    public NadeoAPIResponseException(string? message, Exception? innerException)
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
