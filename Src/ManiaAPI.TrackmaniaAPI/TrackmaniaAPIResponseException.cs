using System.Net;

namespace ManiaAPI.TrackmaniaAPI;

public class TrackmaniaAPIResponseException : Exception
{
    public ErrorResponse? Response { get; }
    public HttpStatusCode StatusCode { get; }
    public string? ReasonPhrase { get; }

    public TrackmaniaAPIResponseException(ErrorResponse? response, HttpStatusCode statusCode, string? reasonPhrase)
        : this(response?.Message ?? "No message received from TrackmaniaAPI")
    {
        Response = response;
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
    }

    public TrackmaniaAPIResponseException(string? message) : base(message) { }
    public TrackmaniaAPIResponseException(string? message, Exception? innerException) : base(message, innerException) { }
}
