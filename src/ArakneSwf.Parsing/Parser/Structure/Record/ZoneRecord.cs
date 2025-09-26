namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// A zone alignment record consisting of two ZoneData entries and X/Y masks.
/// </summary>
public sealed class ZoneRecord
{
    /// <summary>
    /// Should contain exactly two entries in practice (per SWF spec), but we read the declared count.
    /// </summary>
    public IReadOnlyList<ZoneData> Data { get; }

    public bool MaskY { get; }
    public bool MaskX { get; }

    public ZoneRecord(IReadOnlyList<ZoneData> data, bool maskY, bool maskX)
    {
        Data = data;
        MaskY = maskY;
        MaskX = maskX;
    }

    /// <summary>
    /// Read ZoneRecord entries until the given end offset.
    /// </summary>
    /// <param name="reader">Base reader (its cursor will be advanced to <paramref name="end"/>).</param>
    /// <param name="end">End byte offset (exclusive).</param>
    public static List<ZoneRecord> ReadCollection(SwfReader reader, int end)
    {
        var records = new List<ZoneRecord>();

        // Work on a bounded sub-reader while moving the main cursor to 'end'
        var chunk = reader.Chunk(reader.Offset, end);
        reader.SkipTo(end);

        while (chunk.Offset < end)
        {
            int count = chunk.ReadUi8(); // typically 2
            var data = new List<ZoneData>(count);

            for (var i = 0; i < count; ++i)
            {
                var alignmentCoordinate = chunk.ReadFloat16();
                var range = chunk.ReadFloat16();
                data.Add(new ZoneData(alignmentCoordinate, range));
            }

            var flags = chunk.ReadUi8();
            // 6 bits reserved
            var maskY = (flags & 0b0000_0010) != 0;
            var maskX = (flags & 0b0000_0001) != 0;

            records.Add(new ZoneRecord(data, maskY, maskX));
        }

        return records;
    }
}