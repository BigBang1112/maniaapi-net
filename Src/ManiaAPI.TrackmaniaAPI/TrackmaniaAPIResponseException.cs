namespace ManiaAPI.TrackmaniaAPI;

public class TrackmaniaAPIResponseException : Exception
{
    public ErrorResponse? Response { get; }

    public TrackmaniaAPIResponseException(ErrorResponse? response, HttpRequestException inner)
        : this(response?.Message ?? "No message received from TrackmaniaAPI", inner)
    {
        Response = response;
    }

    public TrackmaniaAPIResponseException(string message) : base(message) { }
    public TrackmaniaAPIResponseException(string message, Exception inner) : base(message, inner) { }
}
