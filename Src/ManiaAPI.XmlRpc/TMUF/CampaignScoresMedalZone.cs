namespace ManiaAPI.XmlRpc.TMUF;

public sealed class CampaignScoresMedalZone(Dictionary<PlayMode, RecordUnit[]> medals)
{
    public IReadOnlyDictionary<PlayMode, RecordUnit[]> Medals { get; } = medals;
}