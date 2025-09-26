using System.Text;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Shape;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineFont2 (TYPE = 48) / DefineFont3 (TYPE = 75).
/// </summary>
public sealed class DefineFont2Or3Tag
{
    public const int TypeV2 = 48;
    public const int TypeV3 = 75;

    public int Version { get; }
    public int FontId { get; }
    public bool FontFlagsShiftJis { get; }
    public bool FontFlagsSmallText { get; }
    public bool FontFlagsAnsi { get; }
    public bool FontFlagsWideCodes { get; }
    public bool FontFlagsItalic { get; }
    public bool FontFlagsBold { get; }
    public int LanguageCode { get; }
    public string FontName { get; }
    public int NumGlyphs { get; }

    public IReadOnlyList<int> OffsetTable { get; }
    public IReadOnlyList<IReadOnlyList<ShapeRecord>> GlyphShapeTable { get; }
    public IReadOnlyList<int> CodeTable { get; }
    public FontLayout? Layout { get; }

    public DefineFont2Or3Tag(
        int                                       version,
        int                                       fontId,
        bool                                      fontFlagsShiftJis,
        bool                                      fontFlagsSmallText,
        bool                                      fontFlagsAnsi,
        bool                                      fontFlagsWideCodes,
        bool                                      fontFlagsItalic,
        bool                                      fontFlagsBold,
        int                                       languageCode,
        string                                    fontName,
        int                                       numGlyphs,
        IReadOnlyList<int>                        offsetTable,
        IReadOnlyList<IReadOnlyList<ShapeRecord>> glyphShapeTable,
        IReadOnlyList<int>                        codeTable,
        FontLayout?                               layout)
    {
        Version = version;
        FontId = fontId;
        FontFlagsShiftJis = fontFlagsShiftJis;
        FontFlagsSmallText = fontFlagsSmallText;
        FontFlagsAnsi = fontFlagsAnsi;
        FontFlagsWideCodes = fontFlagsWideCodes;
        FontFlagsItalic = fontFlagsItalic;
        FontFlagsBold = fontFlagsBold;
        LanguageCode = languageCode;
        FontName = fontName;
        NumGlyphs = numGlyphs;
        OffsetTable = offsetTable;
        GlyphShapeTable = glyphShapeTable;
        CodeTable = codeTable;
        Layout = layout;
    }

    /// <summary>
    /// Read a DefineFont2 or DefineFont3 tag.
    /// </summary>
    /// <param name="reader">SWF reader.</param>
    /// <param name="version">2 (DefineFont2) or 3 (DefineFont3).</param>
    public static DefineFont2Or3Tag Read(SwfReader reader, int version)
    {
        int fontId = reader.ReadUi16();

        var flags = reader.ReadUi8();
        var fontFlagsHasLayout = (flags & 0b1000_0000) != 0;
        var fontFlagsShiftJis = (flags & 0b0100_0000) != 0;
        var fontFlagsSmallText = (flags & 0b0010_0000) != 0;
        var fontFlagsAnsi = (flags & 0b0001_0000) != 0;
        var fontFlagsWideOffsets = (flags & 0b0000_1000) != 0;
        var fontFlagsWideCodes = ((flags & 0b0000_0100) != 0) || version > 2; // always wide in v3
        var fontFlagsItalic = (flags & 0b0000_0010) != 0;
        var fontFlagsBold = (flags & 0b0000_0001) != 0;

        int languageCode = reader.ReadUi8();
        int fontNameLength = reader.ReadUi8();

        // Name is length-prefixed and null-terminated; trim the trailing NULL.
        var nameBytes = reader.ReadBytes(fontNameLength);
        var nameLen = nameBytes.Length > 0 ? nameBytes.Length - 1 : 0;
        var fontName = Encoding.Latin1.GetString(nameBytes, 0, nameLen);

        int numGlyphs = reader.ReadUi16();

        var offsetTable = new List<int>(numGlyphs);
        for (var i = 0; i < numGlyphs; i++)
            offsetTable.Add((int)(fontFlagsWideOffsets ? reader.ReadUi32() : reader.ReadUi16()));

        // Skip CodeTableOffset (not used)
        if (fontFlagsWideOffsets) reader.SkipBytes(4);
        else reader.SkipBytes(2);

        // Glyph shapes (always version 1 for ShapeRecord parsing here)
        var glyphShapeTable = new List<IReadOnlyList<ShapeRecord>>(numGlyphs);
        for (var i = 0; i < numGlyphs; i++)
            glyphShapeTable.Add(ShapeRecord.ReadCollection(reader, version: 1));

        // Code table
        var codeTable = new List<int>(numGlyphs);
        for (var i = 0; i < numGlyphs; i++)
            codeTable.Add(fontFlagsWideCodes ? reader.ReadUi16() : reader.ReadUi8());

        // Optional layout
        var layout = fontFlagsHasLayout
            ? FontLayout.Read(reader, numGlyphs, fontFlagsWideCodes)
            : null;

        return new DefineFont2Or3Tag(
            version,
            fontId,
            fontFlagsShiftJis,
            fontFlagsSmallText,
            fontFlagsAnsi,
            fontFlagsWideCodes,
            fontFlagsItalic,
            fontFlagsBold,
            languageCode,
            fontName,
            numGlyphs,
            offsetTable,
            glyphShapeTable,
            codeTable,
            layout
        );
    }
}