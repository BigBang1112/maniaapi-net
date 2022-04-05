using System.Net;

namespace ManiaAPI.NadeoAPI.Exceptions;

public class NadeoApiRequestException : HttpRequestException
{
    public ErrorResponse? ErrorResponse { get; }

    public NadeoApiRequestException(string? message, HttpStatusCode? statusCode, ErrorResponse? errorResponse)
        : base(errorResponse?.Message ?? message, inner: null, statusCode)
    {
        ErrorResponse = errorResponse;
    }
}
