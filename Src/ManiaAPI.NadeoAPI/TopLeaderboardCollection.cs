using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record TopLeaderboardCollection(string GroupUid, string MapUid, ImmutableArray<TopLeaderboard> Tops)
{    
    public TopLeaderboard Top => Tops[0];
}
