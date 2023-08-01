using System.IO.Compression;

namespace ManiaAPI.XmlRpc.TMUF;

public class GeneralScores : IScores
{
    public IReadOnlyDictionary<string, Leaderboard> Zones { get; }

    public GeneralScores(Dictionary<string, Leaderboard> zones)
    {
        Zones = zones;
    }

    public static GeneralScores Parse(string fileName)
    {
        using var fs = File.OpenRead(fileName);
        return Parse(fs);
    }

    public static GeneralScores Parse(Stream stream)
    {
        using var gz = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        using var r = new GbxBasedReader(gz);

        var version = r.ReadByte();
        var count = r.ReadByte();

        var zones = new Dictionary<string, Leaderboard>();

        for (var i = 0; i < count; i++)
        {
            var zoneName = r.ReadString();

            var recordUnits = ScoresReadUtils.ReadRecordsBuffer(r);

            var (sizeOfRanksInt, sizeOfSkillpointsInt) = ScoresReadUtils.ArchiveSizesMask2(r);

            // CGameHighScore array
            var highScoreCount = r.ReadInt32();

            var ranks = ScoresReadUtils.ReadIntBuffer(r, highScoreCount, sizeOfRanksInt);
            var skillpoints = ScoresReadUtils.ReadIntBuffer(r, highScoreCount, sizeOfSkillpointsInt);
            var logins = r.ReadArray(highScoreCount, r => r.ReadString());
            var nicknames = r.ReadArray(highScoreCount, r => r.ReadString());

            var highScores = new HighScore[highScoreCount];

            for (var j = 0; j < highScoreCount; j++)
            {
                highScores[j] = new(ranks[j], skillpoints[j], logins[j], nicknames[j]);
            }

            zones.Add(zoneName, new(highScores, recordUnits));
        }
        
        return new GeneralScores(zones);
    }
}
