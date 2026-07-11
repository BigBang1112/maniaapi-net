using System.Net;

namespace ManiaAPI.NadeoAPI;

public class NadeoAPIResponseException : Exception
{
    public ErrorResponse? Response { get; }
    public HttpStatusCode StatusCode { get; }
    public string? ReasonPhrase { get; }

    public NadeoAPIResponseException(ErrorResponse? response, HttpStatusCode statusCode, string? reasonPhrase)
        : this(response?.Message ?? reasonPhrase ?? "No message received from NadeoAPI")
    {
        Response = response;
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
    }

    public NadeoAPIResponseException(string? message) : base(message) { }
    public NadeoAPIResponseException(string? message, Exception? innerException) : base(message, innerException) { }
}
