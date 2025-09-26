using System.Text;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineFontInfo (TYPE = 13) / DefineFontInfo2 (TYPE = 62).
/// </summary>
public sealed class DefineFontInfoTag
{
    public const int TYPE_V1 = 13;
    public const int TYPE_V2 = 62;

    public int Version { get; }
    public int FontId { get; }
    public string FontName { get; }

    public bool FontFlagsSmallText { get; }
    public bool FontFlagsShiftJIS { get; }
    public bool FontFlagsANSI { get; }
    public bool FontFlagsItalic { get; }
    public bool FontFlagsBold { get; }
    public bool FontFlagsWideCodes { get; }

    public IReadOnlyList<int> CodeTable { get; }
    public int? LanguageCode { get; }

    public DefineFontInfoTag(
        int                version,
        int                fontId,
        string             fontName,
        bool               fontFlagsSmallText,
        bool               fontFlagsShiftJIS,
        bool               fontFlagsANSI,
        bool               fontFlagsItalic,
        bool               fontFlagsBold,
        bool               fontFlagsWideCodes,
        IReadOnlyList<int> codeTable,
        int?               languageCode = null)
    {
        Version = version;
        FontId = fontId;
        FontName = fontName;

        FontFlagsSmallText = fontFlagsSmallText;
        FontFlagsShiftJIS = fontFlagsShiftJIS;
        FontFlagsANSI = fontFlagsANSI;
        FontFlagsItalic = fontFlagsItalic;
        FontFlagsBold = fontFlagsBold;
        FontFlagsWideCodes = fontFlagsWideCodes;

        CodeTable = codeTable;
        LanguageCode = languageCode;
    }

    /// <summary>
    /// Read a DefineFontInfo or DefineFontInfo2 tag.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="version">1 for DefineFontInfo, 2 for DefineFontInfo2.</param>
    /// <param name="end">End byte offset (exclusive) for this tag's data.</param>
    public static DefineFontInfoTag Read(SwfReader reader, int version, int end)
    {
        int fontId = reader.ReadUi16();

        // Name is length-prefixed (no trailing NULL in this tag).
        int nameLen = reader.ReadUi8();
        var fontName = Encoding.Latin1.GetString(reader.ReadBytes(nameLen));

        var flags = reader.ReadUi8();
        // top 2 bits reserved
        var smallText = (flags & 0b0010_0000) != 0;
        var shiftJIS = (flags & 0b0001_0000) != 0;
        var ansi = (flags & 0b0000_1000) != 0;
        var italic = (flags & 0b0000_0100) != 0;
        var bold = (flags & 0b0000_0010) != 0;
        var wideCodes = (flags & 0b0000_0001) != 0 || version > 1; // v2 always uses wide codes

        var languageCode = version > 1 ? reader.ReadUi8() : (int?)null;

        var codeTable = wideCodes
            ? ReadWideCodeTable(reader, end)
            : ReadAsciiCodeTable(reader, end);

        return new DefineFontInfoTag(
            version: version,
            fontId: fontId,
            fontName: fontName,
            fontFlagsSmallText: smallText,
            fontFlagsShiftJIS: shiftJIS,
            fontFlagsANSI: ansi,
            fontFlagsItalic: italic,
            fontFlagsBold: bold,
            fontFlagsWideCodes: wideCodes,
            codeTable: codeTable,
            languageCode: languageCode
        );
    }

    private static List<int> ReadWideCodeTable(SwfReader reader, int end)
    {
        var codeTable = new List<int>();
        while (reader.Offset < end)
            codeTable.Add(reader.ReadUi16());
        return codeTable;
    }

    private static List<int> ReadAsciiCodeTable(SwfReader reader, int end)
    {
        var codeTable = new List<int>();
        while (reader.Offset < end)
            codeTable.Add(reader.ReadUi8());
        return codeTable;
    }
}