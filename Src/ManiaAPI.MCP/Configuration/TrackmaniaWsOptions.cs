namespace ManiaAPI.MCP.Configuration;

public sealed class TrackmaniaWsOptions
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public bool Configured => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
