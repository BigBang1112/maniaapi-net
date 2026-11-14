using System.Net;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Thrown when the web services answer with an error status. <see cref="Exception.Message"/> holds the message of the response, like <c>Invalid environment "Foo"</c>.
/// </summary>
public class TrackmaniaWSResponseException : HttpRequestException
{
    public TrackmaniaWSResponseException(string? message, HttpStatusCode statusCode) : base(message, inner: null, statusCode) { }
}
