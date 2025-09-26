namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Font layout information: metrics, advances, bounds and kerning pairs.
/// </summary>
public sealed class FontLayout
{
    public int Ascent { get; }
    public int Descent { get; }
    public int Leading { get; }

    /// <summary>Advance (SI16) per glyph, length = numGlyphs.</summary>
    public IReadOnlyList<int> AdvanceTable { get; }

    /// <summary>Bounds per glyph, length = numGlyphs.</summary>
    public IReadOnlyList<Rectangle> BoundsTable { get; }

    /// <summary>Kerning pairs.</summary>
    public IReadOnlyList<KerningRecord> KerningTable { get; }

    public FontLayout(
        int                          ascent,
        int                          descent,
        int                          leading,
        IReadOnlyList<int>           advanceTable,
        IReadOnlyList<Rectangle>     boundsTable,
        IReadOnlyList<KerningRecord> kerningTable)
    {
        Ascent = ascent;
        Descent = descent;
        Leading = leading;
        AdvanceTable = advanceTable;
        BoundsTable = boundsTable;
        KerningTable = kerningTable;
    }

    /// <summary>
    /// Read a <see cref="FontLayout"/> from the SWF stream.
    /// </summary>
    /// <param name="reader">SWF reader.</param>
    /// <param name="numGlyphs">Number of glyphs in the font.</param>
    /// <param name="wideCodes">If true, kerning codes are UI16; otherwise UI8.</param>
    public static FontLayout Read(SwfReader reader, int numGlyphs, bool wideCodes)
    {
        int ascent = reader.ReadSi16();
        int descent = reader.ReadSi16();
        int leading = reader.ReadSi16();

        var advanceTable = new List<int>(numGlyphs);
        for (int i = 0; i < numGlyphs; ++i)
            advanceTable.Add(reader.ReadSi16());

        var boundsTable = new List<Rectangle>(numGlyphs);
        for (int i = 0; i < numGlyphs; ++i)
            boundsTable.Add(Rectangle.Read(reader));

        int kerningCount = reader.ReadUi16();
        var kerningTable = new List<KerningRecord>(kerningCount);
        for (int i = 0; i < kerningCount; ++i)
        {
            int code1 = wideCodes ? reader.ReadUi16() : reader.ReadUi8();
            int code2 = wideCodes ? reader.ReadUi16() : reader.ReadUi8();
            int adjustment = reader.ReadSi16();

            kerningTable.Add(new KerningRecord(code1, code2, adjustment));
        }

        return new FontLayout(ascent, descent, leading, advanceTable, boundsTable, kerningTable);
    }
}