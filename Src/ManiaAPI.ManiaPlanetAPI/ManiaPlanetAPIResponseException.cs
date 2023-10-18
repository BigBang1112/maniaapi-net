namespace ManiaAPI.ManiaPlanetAPI;

public class ManiaPlanetAPIResponseException : Exception
{
    public ErrorResponse? Response { get; }

    public ManiaPlanetAPIResponseException(ErrorResponse? response, HttpRequestException inner)
        : this(response?.Message ?? "No message received from ManiaPlanetAPI", inner)
    {
        Response = response;
    }

    public ManiaPlanetAPIResponseException(string message) : base(message) { }
    public ManiaPlanetAPIResponseException(string message, Exception inner) : base(message, inner) { }
}
