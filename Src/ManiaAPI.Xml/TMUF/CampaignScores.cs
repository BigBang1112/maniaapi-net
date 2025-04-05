using System.IO.Compression;

namespace ManiaAPI.Xml.TMUF;

public sealed class CampaignScores(
    IReadOnlyDictionary<string, CampaignScoresLeaderboard> maps,
    IReadOnlyDictionary<string, CampaignScoresMedalZone> medalZones) : IScores
{
    public IReadOnlyDictionary<string, CampaignScoresLeaderboard> Maps { get; } = maps;
    public IReadOnlyDictionary<string, CampaignScoresMedalZone> MedalZones { get; } = medalZones;

    public static CampaignScores Parse(string fileName)
    {
        using var fs = File.OpenRead(fileName);
        return Parse(fs);
    }

    public static CampaignScores Parse(Stream stream)
    {
        using var gz = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        using var r = new GbxBasedReader(gz);

        var version = r.ReadByte(); // 7
        var requestedZoneCount = r.ReadByte(); // 1

        if (requestedZoneCount != 1)
        {
            // The game code actually has a for loop on the amount of zones the gz hold
            // but I haven't seen it done in practice, so I simplified it here
            throw new NotSupportedException("Cannot read more than one requested zone");
        }

        var requestedZoneName = r.ReadString();

        var mapCount = r.ReadInt32();

        var maps = new Dictionary<string, CampaignScoresLeaderboard>(mapCount);

        for (var i = 0; i < mapCount; i++)
        {
            var mapUid = r.ReadString();

            // CGameChallengeScores
            var zoneCount = r.ReadByte();

            var challengeScoresDict = new Dictionary<string, Leaderboard>(zoneCount);

            for (var j = 0; j < zoneCount; j++)
            {
                var zoneName = r.ReadString();
                var hasRecordUnits = r.ReadBoolean(asByte: true);

                var recordUnits = hasRecordUnits ? ScoresReadUtils.ReadRecordsBuffer(r) : [];

                var (sizeOfRanksInt, sizeOfTimesInt) = ScoresReadUtils.ArchiveSizesMask2(r);

                var highScoreCount = r.ReadInt32();

                var ranks = ScoresReadUtils.ReadIntBuffer(r, highScoreCount, sizeOfRanksInt);
                var times = ScoresReadUtils.ReadIntBuffer(r, highScoreCount, sizeOfTimesInt);
                var logins = r.ReadArray(highScoreCount, r => r.ReadString());
                var nicknames = r.ReadArray(highScoreCount, r => r.ReadString());

                var highScores = new HighScore[highScoreCount];

                for (var k = 0; k < highScoreCount; k++)
                {
                    highScores[k] = new(ranks[k], times[k], logins[k], nicknames[k]);
                }

                challengeScoresDict.Add(zoneName, new(highScores, recordUnits));
            }

            maps.Add(mapUid, new CampaignScoresLeaderboard(challengeScoresDict));
        }

        var medalCount = r.ReadByte();

        var medalZones = new Dictionary<string, CampaignScoresMedalZone>();

        for (var i = 0; i < medalCount; i++)
        {
            var zoneName = r.ReadString();
            var modeCount = r.ReadByte();

            var medalZonesDict = new Dictionary<PlayMode, RecordUnit<uint>[]>(modeCount);

            for (var j = 0; j < modeCount; j++)
            {
                var playMode = (PlayMode)r.ReadByte(); // play mode
                var medals = ScoresReadUtils.ReadRecordsBuffer(r);

                medalZonesDict.Add(playMode, medals);
            }

            medalZones.Add(zoneName, new CampaignScoresMedalZone(medalZonesDict));
        }

        return new CampaignScores(maps, medalZones);
    }
}
