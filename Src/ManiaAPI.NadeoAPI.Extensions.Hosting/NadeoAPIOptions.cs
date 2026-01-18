namespace ManiaAPI.NadeoAPI.Extensions.Hosting;

public sealed class NadeoAPIOptions
{
    public NadeoAPICredentials? Credentials { get; set; }

    /// <summary>
    /// User-Agent appended to all NadeoAPI clients.
    /// </summary>
    public string? UserAgent { get; set; }
}
