namespace ManiaAPI.ManiaPlanetAPI;

[Obsolete("ManiaPlanetAPI never throws this exception. It will be removed in a future version.")]
public class MissingRefreshTokenException : Exception
{
    public ErrorResponse? Response { get; }

    public MissingRefreshTokenException() : base("No refresh token was provided") { }
    public MissingRefreshTokenException(string? message) : base(message) { }
    public MissingRefreshTokenException(string? message, Exception inner) : base(message, inner) { }
}
