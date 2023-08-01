namespace ManiaAPI.XmlRpc.TMUF;

static class ScoresReadUtils
{    
    public static RecordUnit[] ReadRecordsBuffer(GbxBasedReader r)
    {
        var (sizeOfScoreInt, sizeOfCountsInt) = ArchiveSizesMask2(r);

        var u03 = r.ReadByte();

        // SRecordUnit array

        var recordUnitCount = r.ReadInt32();

        Span<byte> scoreData = r.ReadBytes(recordUnitCount * sizeOfScoreInt);
        Span<byte> countsData = r.ReadBytes(recordUnitCount * sizeOfCountsInt);

        var array = new RecordUnit[recordUnitCount];

        for (var i = 0; i < recordUnitCount; i++)
        {
            var scoreSlice = scoreData.Slice(i * sizeOfScoreInt, sizeOfScoreInt);
            var countSlice = countsData.Slice(i * sizeOfCountsInt, sizeOfCountsInt);

            var score = BitConverter.ToInt32(scoreSlice);
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
            switch (sizeOfInt)
            {
                case 1:
                    array[i] = r.ReadByte();
                    break;
                case 2:
                    array[i] = r.ReadInt16();
                    break;
                case 4:
                    array[i] = r.ReadInt32();
                    break;
                default:
                    throw new ArgumentException($"Invalid sizeOfInt: {sizeOfInt}");
            }
        }

        return array;
    }
}
