namespace ManiaAPI.NadeoAPI;

public record TopLeaderboardCollection(string GroupUid, string MapUid, TopLeaderboard[] Tops)
{    
    public TopLeaderboard Top => Tops[0];
}
