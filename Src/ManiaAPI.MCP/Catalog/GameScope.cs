using ManiaAPI.MCP.Configuration;

namespace ManiaAPI.MCP.Catalog;

public sealed class GameScope
{
    // Item Exchange has items for these games only, named as their MX sites (ShootMania items are in the Storm collection)
    private static readonly string[] ItemExchangeGames = ["Trackmania", "Maniaplanet", "Shootmania"];

    public IReadOnlySet<string> Toolsets { get; }
    public string[] TmxSites { get; }
    public string[] MxSites { get; }
    public bool InferSite { get; }

    /// <summary>
    /// Games of this scope that Item Exchange has items for, named as MX sites; empty when it's disabled or out of scope.
    /// </summary>
    public string[] ItemExchangeSites { get; }

    /// <summary>
    /// Whether Item Exchange operations are in scope.
    /// </summary>
    public bool ItemExchange => ItemExchangeSites.Length > 0;

    private GameScope(IEnumerable<string> toolsets, string[] tmxSites, string[] mxSites, bool inferSite, string[] itemExchangeSites)
    {
        Toolsets = toolsets.ToHashSet(StringComparer.Ordinal);
        TmxSites = tmxSites;
        MxSites = mxSites;
        InferSite = inferSite;
        ItemExchangeSites = itemExchangeSites;
    }

    public static GameScope Create(McpOptions options, string? endpoint = null)
    {
        var exchange = options.Toolsets.Contains("exchange");
        var itemExchange = exchange && options.Exchange.ItemExchange;

        if (endpoint is null or "mcp" or "exchange")
        {
            var toolsets = endpoint == "exchange" ? options.Toolsets.Where(x => x == "exchange") : options.Toolsets;
            return new(toolsets, exchange ? options.Exchange.Sites : [],
                exchange ? options.Exchange.MxSites : [], false, itemExchange ? ItemExchangeGames : []);
        }
        if (!options.Toolsets.Contains(endpoint))
        {
            throw new ArgumentException("This endpoint's toolset is disabled.");
        }

        var game = GameAliases.Resolve(endpoint);
        var tmx = exchange ? options.Exchange.Sites.Intersect(game.TmxSites).ToArray() : [];
        var mx = exchange ? options.Exchange.MxSites.Intersect(game.MxSites).ToArray() : [];
        var ix = itemExchange ? ItemExchangeGames.Intersect(game.MxSites).ToArray() : [];

        return new(tmx.Length + mx.Length + ix.Length > 0 ? [endpoint, "exchange"] : [endpoint], tmx, mx, true, ix);
    }

    public GameScope ForGame(string game)
    {
        var context = GameAliases.Resolve(game);
        var tmx = TmxSites.Intersect(context.TmxSites).ToArray();
        var mx = MxSites.Intersect(context.MxSites).ToArray();
        var ix = ItemExchangeSites.Intersect(context.MxSites).ToArray();

        if (!Toolsets.Contains(context.Toolset) && tmx.Length + mx.Length + ix.Length == 0)
        {
            throw new ArgumentException($"{game} uses the {context.Toolset} toolset. Use /mcp/{context.Toolset} or /mcp with that toolset enabled.");
        }

        return new(Toolsets.Where(x => x == context.Toolset || x == "exchange"), tmx, mx, InferSite, ix);
    }

    public string[] Sites(Operation operation) => operation.ExchangeClient == "tmx" ? TmxSites : MxSites;

    // Item Exchange is a single site, so its operations have no site parameter and are scoped by ItemExchange instead
    public bool Contains(Operation operation) => Toolsets.Contains(operation.Toolset) &&
        (operation.Id.StartsWith("ix.", StringComparison.Ordinal) ? ItemExchange
            : operation.ExchangeClient is null || Sites(operation).Length > 0);

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
