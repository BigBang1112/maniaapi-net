﻿namespace ManiaAPI.Xml.TMUF;

public sealed class CampaignScoresMedalZone(Dictionary<PlayMode, RecordUnit<uint>[]> medals)
{
    public IReadOnlyDictionary<PlayMode, RecordUnit<uint>[]> Medals { get; } = medals;
}