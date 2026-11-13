namespace ManiaAPI.MCP.Configuration;

public sealed class NadeoOptions
{
    public string? Login { get; set; }
    public string? Password { get; set; }
    public bool Configured => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
}
