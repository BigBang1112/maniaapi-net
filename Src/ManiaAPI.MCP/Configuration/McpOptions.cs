namespace ManiaAPI.MCP.Configuration;

public sealed class McpOptions
{
    public const string Section = "ManiaAPI";
    public static readonly string[] SupportedToolsets =
        ["trackmania2020", "exchange", "tmuf", "tmnf", "maniaplanet", "trackmaniaturbo"];

    public string[] Toolsets { get; set; } = [.. SupportedToolsets];
    public bool AllowWrites { get; set; }

    /// <summary>
    /// Whether a write may go ahead on <c>confirm: true</c> when the client can't show the server's approval prompt (MRTR).
    /// The model sets that argument itself, so this relies only on the client's own tool approval. Off by default,
    /// which refuses writes from such clients.
    /// </summary>
    public bool TrustClientApproval { get; set; }
    public bool ExposeOperationsAsTools { get; set; }

    /// <summary>
    /// Required. Identifies this deployment to every upstream service, like <c>my-assistant/1.0 (contact: you@example.com)</c>,
    /// so their operators can tell deployments apart and reach whoever runs one. The libraries append their own
    /// product and contact details after it.
    /// </summary>
    public string? UserAgent { get; set; }

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
