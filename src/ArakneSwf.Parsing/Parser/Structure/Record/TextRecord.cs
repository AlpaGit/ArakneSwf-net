namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Text record (DefineText / DefineText2).
/// </summary>
public sealed class TextRecord
{
    /// <summary>Should always be 1.</summary>
    public int Type { get; }

    public int? FontId { get; }
    public Color? Color { get; }
    public int? XOffset { get; }
    public int? YOffset { get; }

    /// <summary>Text height in twips (1/20 px). Present only when <see cref="FontId"/> is set.</summary>
    public int? Height { get; }

    public IReadOnlyList<GlyphEntry> Glyphs { get; }

    public TextRecord(
        int                       type,
        int?                      fontId,
        Color?                    color,
        int?                      xOffset,
        int?                      yOffset,
        int?                      height,
        IReadOnlyList<GlyphEntry> glyphs)
    {
        Type = type;
        FontId = fontId;
        Color = color;
        XOffset = xOffset;
        YOffset = yOffset;
        Height = height;
        Glyphs = glyphs ?? new List<GlyphEntry>(0);
    }

    /// <summary>
    /// Reads a collection of <see cref="TextRecord"/> until an empty flag byte (0) is encountered.
    /// </summary>
    /// <param name="reader">SWF reader.</param>
    /// <param name="glyphBits">Number of bits for glyph index (0..32).</param>
    /// <param name="advanceBits">Number of bits for advance (0..32).</param>
    /// <param name="withAlpha">Use RGBA color (DefineText2) when true, RGB otherwise.</param>
    public static List<TextRecord> ReadCollection(SwfReader reader, int glyphBits, int advanceBits, bool withAlpha)
    {
        var records = new List<TextRecord>();

        while (reader.Offset < reader.End)
        {
            var flags = reader.ReadUi8();
            if (flags == 0)
                break;

            var type = flags >> 7;                        // highest bit
            var hasFont = (flags & 0b0000_1000) != 0;    // bit 3
            var hasColor = (flags & 0b0000_0100) != 0;   // bit 2
            var hasYOffset = (flags & 0b0000_0010) != 0; // bit 1
            var hasXOffset = (flags & 0b0000_0001) != 0; // bit 0

            var fontId = hasFont ? reader.ReadUi16() : (int?)null;
            var color = hasColor
                ? (withAlpha ? Color.ReadRgba(reader) : Color.ReadRgb(reader))
                : null;

            var xOffset = hasXOffset ? reader.ReadSi16() : (int?)null;
            var yOffset = hasYOffset ? reader.ReadSi16() : (int?)null;
            var height = hasFont ? reader.ReadUi16() : (int?)null;

            var glyphs = GlyphEntry.ReadCollection(reader, glyphBits, advanceBits);

            records.Add(new TextRecord(type, fontId, color, xOffset, yOffset, height, glyphs));

            reader.AlignByte(); // align after bit-level glyph reads
        }

        return records;
    }
}