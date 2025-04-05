namespace ManiaAPI.Xml.TMUF;

internal static class ScoresReadUtils
{    
    public static RecordUnit<uint>[] ReadRecordsBuffer(GbxBasedReader r)
    {
        var (sizeOfScoreInt, sizeOfCountsInt) = ArchiveSizesMask2(r);

        var u03 = r.ReadByte();

        // SRecordUnit array

        var recordUnitCount = r.ReadInt32();

        Span<byte> scoreData = r.ReadBytes(recordUnitCount * sizeOfScoreInt);
        Span<byte> countsData = r.ReadBytes(recordUnitCount * sizeOfCountsInt);

        var array = new RecordUnit<uint>[recordUnitCount];

        for (var i = 0; i < recordUnitCount; i++)
        {
            var scoreSlice = scoreData.Slice(i * sizeOfScoreInt, sizeOfScoreInt);
            var countSlice = countsData.Slice(i * sizeOfCountsInt, sizeOfCountsInt);

            var score = BitConverter.ToUInt32(scoreSlice);
            var count = BitConverter.ToInt32(countSlice);

            array[i] = new(score, count);
        }
        
        return array;
    }

    public static (int, int) ArchiveSizesMask2(GbxBasedReader r)
    {
        var val = r.ReadByte(); // ArchiveSizesMask2
        return ((val & 3) + 1, (val >> 2) + 1);
    }

    public static int[] ReadIntBuffer(GbxBasedReader r, int count, int sizeOfInt)
    {
        var array = new int[count];

        for (var i = 0; i < count; i++)
        {
            array[i] = sizeOfInt switch
            {
                1 => r.ReadByte(),
                2 => r.ReadInt16(),
                4 => r.ReadInt32(),
                _ => throw new ArgumentException($"Invalid sizeOfInt: {sizeOfInt}"),
            };
        }

        return array;
    }
}
