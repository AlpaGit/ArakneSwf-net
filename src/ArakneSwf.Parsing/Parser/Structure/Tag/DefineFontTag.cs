using ArakneSwf.Parsing.Parser.Structure.Record.Shape;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineFont (TYPE = 10).
/// Maps glyph indices to shapes for a font (SWF v1 style font definition).
/// </summary>
public sealed class DefineFontTag
{
    public const int TYPE_V1 = 10;

    public int FontId { get; }
    public IReadOnlyList<int> OffsetTable { get; }
    public IReadOnlyList<IReadOnlyList<ShapeRecord>> GlyphShapeData { get; }

    public DefineFontTag(
        int                                       fontId,
        IReadOnlyList<int>                        offsetTable,
        IReadOnlyList<IReadOnlyList<ShapeRecord>> glyphShapeData)
    {
        FontId = fontId;
        OffsetTable = offsetTable;
        GlyphShapeData = glyphShapeData;
    }

    /// <summary>
    /// Read a DefineFont tag from the reader.
    /// </summary>
    public static DefineFontTag Read(SwfReader reader)
    {
        int fontId = reader.ReadUi16();

        // First offset points to the first glyph. Each offset entry is 2 bytes,
        // so number of glyphs = firstOffset / 2.
        int numGlyphs = reader.PeekUi16() >> 1;

        var offsetTable = new List<int>(numGlyphs);
        for (int i = 0; i < numGlyphs; i++)
            offsetTable.Add(reader.ReadUi16());

        var glyphShapeData = new List<IReadOnlyList<ShapeRecord>>(numGlyphs);
        for (int i = 0; i < numGlyphs; i++)
            glyphShapeData.Add(ShapeRecord.ReadCollection(reader, version: 1));

        return new DefineFontTag(fontId, offsetTable, glyphShapeData);
    }
}