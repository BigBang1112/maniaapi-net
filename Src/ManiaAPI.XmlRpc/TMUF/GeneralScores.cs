using System.IO.Compression;

namespace ManiaAPI.XmlRpc.TMUF;

public class GeneralScores
{
    public Dictionary<string, GeneralScoresLeaderboard> Zones { get; }

    public GeneralScores(Dictionary<string, GeneralScoresLeaderboard> zones)
    {
        Zones = zones;
    }

    public static GeneralScores Parse(Stream stream)
    {
        using var gz = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        using var r = new GbxBasedReader(gz);

        var version = r.ReadByte();
        var count = r.ReadByte();

        var zones = new Dictionary<string, GeneralScoresLeaderboard>();

        for (var i = 0; i < count; i++)
        {
            var zoneName = r.ReadString();

            var recordUnits = ReadRecordsBuffer(r);

            var (sizeOfRanksInt, sizeOfSkillpointsInt) = ArchiveSizesMask2(r);

            // CGameHighScore array
            var highScoreCount = r.ReadInt32();

            var ranks = ReadIntBuffer(r, highScoreCount, sizeOfRanksInt);
            var skillpoints = ReadIntBuffer(r, highScoreCount, sizeOfSkillpointsInt);
            var logins = r.ReadArray(highScoreCount, r => r.ReadString());
            var nicknames = r.ReadArray(highScoreCount, r => r.ReadString());

            var highScores = new GeneralScoresHighScore[highScoreCount];

            for (var j = 0; j < highScoreCount; j++)
            {
                highScores[j] = new(ranks[j], skillpoints[j], logins[j], nicknames[j]);
            }

            zones.Add(zoneName, new(highScores, recordUnits));
        }

        return new GeneralScores(zones);
    }

    private static GeneralScoresRecordUnit[] ReadRecordsBuffer(GbxBasedReader r)
    {
        var (sizeOfSkillpointsInt, sizeOfCountsInt) = ArchiveSizesMask2(r);

        var u03 = r.ReadByte();

        // SRecordUnit array

        var recordUnitCount = r.ReadInt32();

        Span<byte> skillpointsData = r.ReadBytes(recordUnitCount * sizeOfSkillpointsInt);
        Span<byte> countsData = r.ReadBytes(recordUnitCount * sizeOfCountsInt);

        var array = new GeneralScoresRecordUnit[recordUnitCount];

        for (var i = 0; i < recordUnitCount; i++)
        {
            var skillpointsSlice = skillpointsData.Slice(i * sizeOfSkillpointsInt, sizeOfSkillpointsInt);
            var countSlice = countsData.Slice(i * sizeOfCountsInt, sizeOfCountsInt);

            var skillpoints = BitConverter.ToInt32(skillpointsSlice);
            var count = BitConverter.ToInt32(countSlice);

            array[i] = new(skillpoints, count);
        }

        return array;
    }

    private static int[] ReadIntBuffer(GbxBasedReader r, int count, int sizeOfInt)
    {
        Span<byte> data = stackalloc byte[count * sizeOfInt];

        if (r.Read(data) != data.Length)
        {
            throw new EndOfStreamException();
        }

        var array = new int[count];

        for (var i = 0; i < count; i++)
        {
            var slice = data.Slice(i * sizeOfInt, sizeOfInt);
            array[i] = BitConverter.ToInt32(slice);
        }

        return array;
    }

    private static (int, int) ArchiveSizesMask2(GbxBasedReader r)
    {
        var val = r.ReadByte(); // ArchiveSizesMask2
        return ((val & 3) + 1, (val >> 2) + 1);
    }
}
