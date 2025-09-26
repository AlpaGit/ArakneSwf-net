namespace ArakneSwf.Parsing.Parser.Structure.Record;

public sealed class SoundInfo
{
    public bool SyncStop { get; }
    public bool SyncNoMultiple { get; }
    public int? InPoint { get; }
    public int? OutPoint { get; }
    public int? LoopCount { get; }
    public IReadOnlyList<SoundEnvelope> Envelopes { get; }

    public SoundInfo(
        bool                         syncStop,
        bool                         syncNoMultiple,
        int?                         inPoint,
        int?                         outPoint,
        int?                         loopCount,
        IReadOnlyList<SoundEnvelope> envelopes)
    {
        SyncStop = syncStop;
        SyncNoMultiple = syncNoMultiple;
        InPoint = inPoint;
        OutPoint = outPoint;
        LoopCount = loopCount;
        Envelopes = envelopes ?? new List<SoundEnvelope>(0);
    }

    /// <summary>Read a single SoundInfo record.</summary>
    public static SoundInfo Read(SwfReader reader)
    {
        byte flags = reader.ReadUi8();
        // 2 bits reserved
        bool syncStop       = (flags & 0b0010_0000) != 0;
        bool syncNoMultiple = (flags & 0b0001_0000) != 0;
        bool hasEnvelope    = (flags & 0b0000_1000) != 0;
        bool hasLoops       = (flags & 0b0000_0100) != 0;
        bool hasOutPoint    = (flags & 0b0000_0010) != 0;
        bool hasInPoint     = (flags & 0b0000_0001) != 0;

        int? inPoint   = hasInPoint  ? (int?)reader.ReadUi32() : (int?)null;
        int? outPoint  = hasOutPoint ? (int?)reader.ReadUi32() : (int?)null;
        int? loopCount = hasLoops    ? reader.ReadUi16() : (int?)null;

        var envelopes = hasEnvelope ? ReadEnvelopes(reader) : new List<SoundEnvelope>(0);

        return new SoundInfo(syncStop, syncNoMultiple, inPoint, outPoint, loopCount, envelopes);
    }

    private static List<SoundEnvelope> ReadEnvelopes(SwfReader reader)
    {
        int count = reader.ReadUi8();
        var list = new List<SoundEnvelope>(count);

        for (int i = 0; i < count; i++)
        {
            var pos44      = (int)reader.ReadUi32();
            int leftLevel  = reader.ReadUi16();
            int rightLevel = reader.ReadUi16();
            list.Add(new SoundEnvelope(pos44, leftLevel, rightLevel));
        }

        return list;
    }
}