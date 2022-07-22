namespace ManiaAPI.XmlRpc.TMUF;

public class CampaignScoresMedalZone
{
    public IReadOnlyDictionary<PlayMode, RecordUnit[]> Medals { get; }

    public CampaignScoresMedalZone(Dictionary<PlayMode, RecordUnit[]> medals)
    {
        Medals = medals;
    }
}