namespace ManiaAPI.ManiaPlanetAPI;

public class MissingRefreshTokenException : Exception
{
    public ErrorResponse? Response { get; }

    public MissingRefreshTokenException() : base("No refresh token was provided") { }
    public MissingRefreshTokenException(string message) : base(message) { }
    public MissingRefreshTokenException(string message, Exception inner) : base(message, inner) { }
}
