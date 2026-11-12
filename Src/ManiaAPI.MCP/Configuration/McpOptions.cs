namespace ManiaAPI.MCP.Configuration;

public sealed class McpOptions
{
    public const string Section = "ManiaAPI";
    public static readonly string[] SupportedToolsets =
        ["trackmania2020", "exchange", "tmuf", "tmnf", "maniaplanet", "trackmaniaturbo"];

    public string[] Toolsets { get; set; } = [.. SupportedToolsets];
    public bool AllowWrites { get; set; }
    public bool ExposeOperationsAsTools { get; set; }
    public HttpOptions Http { get; set; } = new();
    public ShapingOptions Shaping { get; set; } = new();
    public NadeoOptions NadeoAPI { get; set; } = new();
    public OAuthOptions TrackmaniaAPI { get; set; } = new();
    public OAuthOptions ManiaPlanet { get; set; } = new();
    public TrackmaniaWsOptions TrackmaniaWS { get; set; } = new();
    public MasterServerOptions MasterServer { get; set; } = new();
    public ExchangeOptions Exchange { get; set; } = new();
    public Dictionary<string, FeaturedOptions> Featured { get; set; } = [];
    public AccountLinkingOptions AccountLinking { get; set; } = new();
}
