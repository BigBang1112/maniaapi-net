using System.Net;

namespace ManiaAPI.Base.Exceptions;

public class ApiRequestException : HttpRequestException
{
    public ErrorResponse? ErrorResponse { get; }

    public ApiRequestException(string? message, HttpStatusCode? statusCode, ErrorResponse? errorResponse)
        : base(errorResponse?.Message ?? message, inner: null, statusCode)
    {
        ErrorResponse = errorResponse;
    }
}
