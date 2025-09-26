using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineFontAlignZones tag (TYPE = 73).
/// </summary>
public sealed class DefineFontAlignZonesTag
{
    public const int TYPE = 73;

    public int FontId { get; }

    /// <summary>2-bit hint value extracted from flags (CSMTableHint).</summary>
    public int CsmTableHint { get; }

    public IReadOnlyList<ZoneRecord> ZoneTable { get; }

    public DefineFontAlignZonesTag(int fontId, int csmTableHint, IReadOnlyList<ZoneRecord> zoneTable)
    {
        FontId = fontId;
        CsmTableHint = csmTableHint;
        ZoneTable = zoneTable;
    }

    /// <summary>
    /// Read a DefineFontAlignZones tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of the tag data.</param>
    public static DefineFontAlignZonesTag Read(SwfReader reader, int end)
    {
        int fontId = reader.ReadUi16();
        var flags = reader.ReadUi8();

        var csmTableHint = (flags >> 6) & 0b11; // top 2 bits
        // remaining 6 bits are reserved

        var zoneTable = ZoneRecord.ReadCollection(reader, end);

        return new DefineFontAlignZonesTag(fontId, csmTableHint, zoneTable);
    }
}