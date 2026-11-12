using Microsoft.Extensions.Options;

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

public sealed class AccountLinkingOptions
{
    public bool Enabled { get; set; }
    public string? StorePath { get; set; }
    public string? DataProtectionKeyPath { get; set; }
    public string? PublicOrigin { get; set; }
    public string[] TrackmaniaScopes { get; set; } = ["clubs", "read_favorite", "write_favorite"];
    public string[] ManiaPlanetScopes { get; set; } = ["basic", "dedicated", "maps", "titles"];
}

public sealed class FeaturedOptions
{
    public string[]? Tools { get; set; }
}

public sealed class MasterServerOptions
{
    public string[] Games { get; set; } = ["TMUF", "TMNF", "MP3", "MP4", "TMT"];
}

public sealed class TrackmaniaWsOptions
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public bool Configured => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}

public sealed class HttpOptions
{
    public string[] ApiKeys { get; set; } = [];
}

public sealed class ShapingOptions
{
    public int MaxItems { get; set; } = 50;
    public int MaxChars { get; set; } = 20000;
    public int MaxCharsLimit { get; set; } = 100000;
    public int MaxResourceBytes { get; set; } = 5242880;
}

public sealed class NadeoOptions
{
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? UserAgent { get; set; }
    public bool Configured => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
}

public sealed class OAuthOptions
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string[] Scopes { get; set; } = [];
    public bool Configured => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}

public sealed class ExchangeOptions
{
    public string[] Sites { get; set; } = ["TMUF", "TMNF", "Nations", "Sunrise", "Original"];
    public string[] MxSites { get; set; } = ["Trackmania", "Maniaplanet", "Shootmania"];
}

public sealed class McpOptionsValidator : IValidateOptions<McpOptions>
{
    public ValidateOptionsResult Validate(string? name, McpOptions options)
    {
        List<string> errors = [];

        if (options.Toolsets.Any(x => !McpOptions.SupportedToolsets.Contains(x, StringComparer.Ordinal)))
        {
            errors.Add("Toolsets must use the supported game or exchange toolset names.");
        }

        if (options.MasterServer.Games.Any(x => x is not ("TMUF" or "TMNF" or "MP3" or "MP4" or "TMT")))
        {
            errors.Add("MasterServer:Games must contain TMUF, TMNF, MP3, MP4, or TMT.");
        }

        if (options.Featured.Keys.Any(x => !McpOptions.SupportedToolsets.Contains(x, StringComparer.Ordinal)))
        {
            errors.Add("Featured must use supported toolset names.");
        }

        if (options.AccountLinking.Enabled &&
            (string.IsNullOrWhiteSpace(options.AccountLinking.StorePath) ||
             string.IsNullOrWhiteSpace(options.AccountLinking.DataProtectionKeyPath) ||
             !Uri.TryCreate(options.AccountLinking.PublicOrigin, UriKind.Absolute, out var origin) ||
             origin.AbsolutePath != "/" || !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment) ||
             !string.IsNullOrEmpty(origin.UserInfo) ||
             (origin.Scheme != "https" && !(origin.Scheme == "http" && origin.IsLoopback))))
        {
            errors.Add("AccountLinking requires StorePath, DataProtectionKeyPath, and an HTTPS PublicOrigin (HTTP is permitted on loopback).");
        }

        if (options.Exchange.Sites.Any(x => !Enum.GetNames<TMX.TmxSite>().Contains(x)) ||
            options.Exchange.MxSites.Any(x => !Enum.GetNames<TMX.MxSite>().Contains(x)))
        {
            errors.Add("Exchange sites must use the existing TmxSite and MxSite enum names.");
        }

        if (options.Shaping.MaxItems is < 1 or > 10000 || options.Shaping.MaxChars < 256 ||
            options.Shaping.MaxCharsLimit < options.Shaping.MaxChars || options.Shaping.MaxCharsLimit > 1000000 ||
            options.Shaping.MaxResourceBytes is < 1 or > 104857600)
        {
            errors.Add("Shaping limits must be positive: MaxItems <= 10000, 256 <= MaxChars <= MaxCharsLimit <= 1000000.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
