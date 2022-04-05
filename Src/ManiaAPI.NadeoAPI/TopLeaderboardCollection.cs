namespace ManiaAPI.NadeoAPI;

public class TopLeaderboardCollection
{
    public string GroupUid { get; init; }
    public string MapUid { get; init; }
    public TopLeaderboard[] Tops { get; init; }
    
    public TopLeaderboard Top => Tops.First();

    public TopLeaderboardCollection(string groupUid, string mapUid, TopLeaderboard[] tops)
    {
        GroupUid = groupUid;
        MapUid = mapUid;
        Tops = tops;
    }
}
