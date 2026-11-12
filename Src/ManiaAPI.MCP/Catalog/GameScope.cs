using ManiaAPI.MCP.Configuration;

namespace ManiaAPI.MCP.Catalog;

public sealed class GameScope
{
    public IReadOnlySet<string> Toolsets { get; }
    public string[] TmxSites { get; }
    public string[] MxSites { get; }
    public bool InferSite { get; }

    private GameScope(IEnumerable<string> toolsets, string[] tmxSites, string[] mxSites, bool inferSite)
    {
        Toolsets = toolsets.ToHashSet(StringComparer.Ordinal);
        TmxSites = tmxSites;
        MxSites = mxSites;
        InferSite = inferSite;
    }

    public static GameScope Create(McpOptions options, string? endpoint = null)
    {
        if (endpoint is null or "mcp" or "exchange")
        {
            var toolsets = endpoint == "exchange" ? options.Toolsets.Where(x => x == "exchange") : options.Toolsets;
            return new(toolsets, options.Toolsets.Contains("exchange") ? options.Exchange.Sites : [],
                options.Toolsets.Contains("exchange") ? options.Exchange.MxSites : [], false);
        }
        if (!options.Toolsets.Contains(endpoint))
        {
            throw new ArgumentException("This endpoint's toolset is disabled.");
        }

        var game = GameAliases.Resolve(endpoint);
        var exchange = options.Toolsets.Contains("exchange");
        var tmx = exchange ? options.Exchange.Sites.Intersect(game.TmxSites).ToArray() : [];
        var mx = exchange ? options.Exchange.MxSites.Intersect(game.MxSites).ToArray() : [];

        return new(tmx.Length + mx.Length > 0 ? [endpoint, "exchange"] : [endpoint], tmx, mx, true);
    }

    public GameScope ForGame(string game)
    {
        var context = GameAliases.Resolve(game);
        var tmx = TmxSites.Intersect(context.TmxSites).ToArray();
        var mx = MxSites.Intersect(context.MxSites).ToArray();

        if (!Toolsets.Contains(context.Toolset) && tmx.Length + mx.Length == 0)
        {
            throw new ArgumentException($"{game} uses the {context.Toolset} toolset. Use /mcp/{context.Toolset} or /mcp with that toolset enabled.");
        }

        return new(Toolsets.Where(x => x == context.Toolset || x == "exchange"), tmx, mx, InferSite);
    }

    public string[] Sites(Operation operation) => operation.ExchangeClient == "tmx" ? TmxSites : MxSites;
    public bool Contains(Operation operation) => Toolsets.Contains(operation.Toolset) &&
        (operation.ExchangeClient is null || Sites(operation).Length > 0);

    public string SelectSite(Operation operation, string? site)
    {
        var sites = Sites(operation);

        if (site is null && InferSite && sites.Length == 1)
        {
            return sites[0];
        }

        if (site is not null && sites.Contains(site, StringComparer.Ordinal))
        {
            return site;
        }

        throw new ArgumentException($"site must be one of: {string.Join(", ", sites)}.");
    }
}
