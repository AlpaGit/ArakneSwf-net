using System.Diagnostics;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Glyph entry used by text tags: (glyph index, advance).
/// </summary>
public sealed class GlyphEntry
{
    /// <summary>Index of the glyph in the font.</summary>
    public int GlyphIndex { get; }

    /// <summary>Advance for this glyph (signed).</summary>
    public int Advance { get; }

    public GlyphEntry(int glyphIndex, int advance)
    {
        GlyphIndex = glyphIndex;
        Advance = advance;
    }

    /// <summary>
    /// Reads a collection of <see cref="GlyphEntry"/>.
    /// First byte gives the number of entries to read.
    /// </summary>
    /// <param name="reader">SWF reader (bit-aware).</param>
    /// <param name="glyphBits">Number of bits for glyph index (0..32).</param>
    /// <param name="advanceBits">Number of bits for advance (0..32).</param>
    public static List<GlyphEntry> ReadCollection(SwfReader reader, int glyphBits, int advanceBits)
    {
        Debug.Assert(glyphBits >= 0 && glyphBits <= 32);
        Debug.Assert(advanceBits >= 0 && advanceBits <= 32);

        int count = reader.ReadUi8();
        var entries = new List<GlyphEntry>(count);

        for (var i = 0; i < count; i++)
        {
            int glyphIndex = (int)reader.ReadUb(glyphBits);
            var advance    = reader.ReadSb(advanceBits);

            entries.Add(new GlyphEntry(glyphIndex, advance));
        }

        return entries;
    }
}
