using System.IO.Compression;

namespace ManiaAPI.Xml.TMUF;

public sealed class LadderScores(IReadOnlyDictionary<string, LadderZone> zones) : IScores
{
    public IReadOnlyDictionary<string, LadderZone> Zones { get; } = zones;

    public static LadderScores Parse(string fileName)
    {
        using var fs = File.OpenRead(fileName);
        return Parse(fs);
    }

    public static LadderScores Parse(Stream stream)
    {
        using var gz = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        using var r = new GbxBasedReader(gz);

        var version = r.ReadByte();
        var count = r.ReadByte();

        var zones = new Dictionary<string, LadderZone>();

        for (var i = 0; i < count; i++)
        {
            var zoneName = r.ReadString();
            var playerCount = r.ReadInt32();
            var ranks = r.ReadArray<int>();
            var points = r.ReadArray<int>(ranks.Length);

            zones.Add(zoneName, new LadderZone(playerCount, ranks, points));
        }
        
        return new LadderScores(zones);
    }
}
