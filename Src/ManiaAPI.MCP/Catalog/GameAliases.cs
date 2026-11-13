using System.Text;

namespace ManiaAPI.MCP.Catalog;

public static class GameAliases
{
    public static GameContext Resolve(string value)
    {
        var name = string.Concat(value.Normalize(NormalizationForm.FormKC).Where(char.IsLetterOrDigit)).ToLowerInvariant();

        return name switch
        {
            "trackmania2020" or "tm2020" or "tm20" => new("trackmania2020", null, [], ["Trackmania"]),
            "tmuf" or "trackmaniaunitedforever" => new("tmuf", null, ["TMUF"], []),
            "tmnf" or "trackmanianationsforever" => new("tmnf", null, ["TMNF"], []),
            "trackmania2" or "tm2" => new("maniaplanet", "Trackmania 2", [], ["Maniaplanet"]),
            "shootmania" or "sm" => new("maniaplanet", "ShootMania", [], ["Shootmania"]),
            "maniaplanet" => new("maniaplanet", null, [], ["Maniaplanet", "Shootmania"]),
            "trackmaniaturbo" or "tmt" or "turbo" => new("trackmaniaturbo", null, [], []),
            "stadium" => throw new ArgumentException("Stadium is ambiguous. Supply the game or explicit title context."),
            _ => throw new ArgumentException("Unknown game. Supported choices: trackmania2020, tmuf, tmnf, maniaplanet, TM2, ShootMania, trackmaniaturbo.")
        };
    }
}
