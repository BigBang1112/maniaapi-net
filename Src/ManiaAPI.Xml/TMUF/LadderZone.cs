namespace ManiaAPI.Xml.TMUF;

public sealed class LadderZone(int playerCount, int[] ranks, int[] points)
{
    public int PlayerCount { get; } = playerCount;
    public int[] Ranks { get; } = ranks;
    public int[] Points { get; } = points;
}
