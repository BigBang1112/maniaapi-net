namespace ManiaAPI.MCP.Configuration;

public sealed class AccountLinkingOptions
{
    public bool Enabled { get; set; }
    public string? StorePath { get; set; }
    public string? DataProtectionKeyPath { get; set; }
    public string? PublicOrigin { get; set; }
    public string[] TrackmaniaScopes { get; set; } = ["clubs", "read_favorite", "write_favorite"];
    public string[] ManiaPlanetScopes { get; set; } = ["basic", "dedicated", "maps", "titles"];
}
