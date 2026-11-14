namespace ManiaAPI.TMX;

/// <summary>
/// Game an Item Exchange item or set is for.
/// </summary>
public enum IXGame
{
    Unknown = -1,

    /// <summary>
    /// Common to multiple games.
    /// </summary>
    Common = 0,

    Maniaplanet = 1,
    Trackmania = 2
}
