using System.Net;

namespace ManiaAPI.ManiaPlanetAPI;

public class ManiaPlanetAPIResponseException : Exception
{
    public ErrorResponse? Response { get; }
    public HttpStatusCode StatusCode { get; }
    public string? ReasonPhrase { get; }

    public ManiaPlanetAPIResponseException(ErrorResponse? response, HttpStatusCode statusCode, string? reasonPhrase)
        : this(response?.Message ?? reasonPhrase ?? statusCode.ToString())
    {
        Response = response;
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
    }

    public ManiaPlanetAPIResponseException(string? message) : base(message) { }
    public ManiaPlanetAPIResponseException(string? message, Exception? innerException) : base(message, innerException) { }
}
