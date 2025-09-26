namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineFont4 tag (TYPE = 91).
/// </summary>
public sealed class DefineFont4Tag
{
    public const int TYPE_V4 = 91;

    public int FontId { get; }
    public bool Italic { get; }
    public bool Bold { get; }
    public string Name { get; }

    /// <summary>Embedded font data (raw bytes) or null if not present.</summary>
    public byte[]? Data { get; }

    public DefineFont4Tag(int fontId, bool italic, bool bold, string name, byte[]? data)
    {
        FontId = fontId;
        Italic = italic;
        Bold = bold;
        Name = name;
        Data = data;
    }

    /// <summary>
    /// Read a DefineFont4 tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag's payload.</param>
    public static DefineFont4Tag Read(SwfReader reader, int end)
    {
        int fontId = reader.ReadUi16();

        var flags = reader.ReadUi8();
        // 5 bits reserved (top bits)
        var fontFlagsHasFontData = (flags & 0b0000_0100) != 0;
        var fontFlagsItalic = (flags & 0b0000_0010) != 0;
        var fontFlagsBold = (flags & 0b0000_0001) != 0;

        var fontName = reader.ReadNullTerminatedString();
        var fontData = fontFlagsHasFontData ? reader.ReadBytesTo(end) : null;

        return new DefineFont4Tag(
            fontId: fontId,
            italic: fontFlagsItalic,
            bold: fontFlagsBold,
            name: fontName,
            data: fontData
        );
    }
}