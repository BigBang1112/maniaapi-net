namespace ManiaAPI.NadeoAPI;

public class NadeoAPIResponseException : Exception
{
    public ErrorResponse? Response { get; }

    public NadeoAPIResponseException(ErrorResponse? response, HttpRequestException inner)
        : this (response?.Message ?? "No message received from NadeoAPI", inner)
    {
        Response = response;
    }

    public NadeoAPIResponseException(string message) : base(message) { }
    public NadeoAPIResponseException(string message, Exception inner) : base(message, inner) { }
}
