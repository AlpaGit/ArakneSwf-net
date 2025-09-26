using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineText / DefineText2 tag.
/// </summary>
public sealed class DefineTextTag
{
    public const int TYPE_V1 = 11;
    public const int TYPE_V2 = 33;

    public int Version { get; }
    public int CharacterId { get; }
    public Rectangle TextBounds { get; }
    public Matrix TextMatrix { get; }
    public int GlyphBits { get; }
    public int AdvanceBits { get; }
    public IReadOnlyList<TextRecord> TextRecords { get; }

    public DefineTextTag(
        int                       version,
        int                       characterId,
        Rectangle                 textBounds,
        Matrix                    textMatrix,
        int                       glyphBits,
        int                       advanceBits,
        IReadOnlyList<TextRecord> textRecords)
    {
        Version = version;
        CharacterId = characterId;
        TextBounds = textBounds;
        TextMatrix = textMatrix;
        GlyphBits = glyphBits;
        AdvanceBits = advanceBits;
        TextRecords = textRecords;
    }

    /// <summary>
    /// Read a DefineText (v1) or DefineText2 (v2) tag from the reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="version">1 or 2. Version 2 enables RGBA colors in TextRecord.</param>
    public static DefineTextTag Read(SwfReader reader, int version)
    {
        int characterId = reader.ReadUi16();
        var textBounds = Rectangle.Read(reader);
        var textMatrix = Matrix.Read(reader);
        int glyphBits = reader.ReadUi8();
        int advanceBits = reader.ReadUi8();

        IReadOnlyList<TextRecord> textRecords;

        if (glyphBits > 32 || advanceBits > 32)
        {
            if ((reader.Errors & Errors.InvalidData) != 0)
            {
                throw new ParserInvalidDataException(
                    $"Glyph bits ({glyphBits}) or advance bits ({advanceBits}) are out of bounds (0-32)",
                    reader.Offset
                );
            }

            textRecords = new List<TextRecord>(capacity: 0);
        }
        else
        {
            bool withAlpha = version > 1;
            textRecords = TextRecord.ReadCollection(reader, glyphBits, advanceBits, withAlpha);
        }

        return new DefineTextTag(
            version: version,
            characterId: characterId,
            textBounds: textBounds,
            textMatrix: textMatrix,
            glyphBits: glyphBits,
            advanceBits: advanceBits,
            textRecords: textRecords
        );
    }
}