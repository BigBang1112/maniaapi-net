using System.Net;

namespace ManiaAPI.Base.Exceptions;

public class ApiRequestException : HttpRequestException
{
    public ApiRequestException(string? message, HttpStatusCode? statusCode) : base(message, inner: null, statusCode)
    {
        
    }
}
