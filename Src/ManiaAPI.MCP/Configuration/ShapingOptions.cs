namespace ManiaAPI.MCP.Configuration;

public sealed class ShapingOptions
{
    public int MaxItems { get; set; } = 50;
    public int MaxChars { get; set; } = 20000;
    public int MaxCharsLimit { get; set; } = 100000;
    public int MaxResourceBytes { get; set; } = 5242880;
}
