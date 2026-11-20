namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

public sealed class TrackmaniaAPIOptions
{
    public TrackmaniaAPICredentials? Credentials { get; set; }

    public string? UserAgent { get; set; }
}
