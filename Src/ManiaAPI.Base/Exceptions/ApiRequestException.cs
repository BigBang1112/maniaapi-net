using System.Net;

namespace ManiaAPI.Base.Exceptions;

public class ApiRequestException<T> : HttpRequestException where T : ErrorResponse
{
    public T? ErrorResponse { get; }

    public ApiRequestException(string? message, HttpStatusCode? statusCode, T? errorResponse)
        : base(errorResponse?.Message ?? message, inner: null, statusCode)
    {
        ErrorResponse = errorResponse;
    }
}
