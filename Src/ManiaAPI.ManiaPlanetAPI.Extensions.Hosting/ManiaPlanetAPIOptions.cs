namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

public sealed class ManiaPlanetAPIOptions
{
    public ManiaPlanetAPICredentials? Credentials { get; set; }

    /// <summary>Application User-Agent sent before the library identity.</summary>
    public string? UserAgent { get; set; }
}
