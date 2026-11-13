namespace ManiaAPI.MCP.Configuration;

public sealed class ExchangeOptions
{
    public string[] Sites { get; set; } = ["TMUF", "TMNF", "Nations", "Sunrise", "Original"];
    public string[] MxSites { get; set; } = ["Trackmania", "Maniaplanet", "Shootmania"];

    /// <summary>
    /// Whether Item Exchange (items and item sets for ManiaPlanet and Trackmania) is available.
    /// </summary>
    public bool ItemExchange { get; set; } = true;
}
