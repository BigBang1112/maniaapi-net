using System.Net;

namespace ManiaAPI.TrackmaniaAPI.Exceptions;

public class TrackmaniaApiRequestException : HttpRequestException
{
    public ErrorResponse? ErrorResponse { get; }

    public TrackmaniaApiRequestException(string? message, HttpStatusCode? statusCode, ErrorResponse? errorResponse)
        : base(errorResponse?.Message ?? message, inner: null, statusCode)
    {
        ErrorResponse = errorResponse;
    }
}
