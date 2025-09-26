namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineFontName tag (TYPE = 88).
/// </summary>
public sealed class DefineFontNameTag
{
    public const int TYPE = 88;

    public int FontId { get; }
    public string FontName { get; }
    public string FontCopyright { get; }

    public DefineFontNameTag(int fontId, string fontName, string fontCopyright)
    {
        FontId = fontId;
        FontName = fontName;
        FontCopyright = fontCopyright;
    }

    /// <summary>
    /// Read a DefineFontName tag from the stream.
    /// </summary>
    public static DefineFontNameTag Read(SwfReader reader)
    {
        int fontId = reader.ReadUi16();
        string fontName = reader.ReadNullTerminatedString();
        string fontCopyright = reader.ReadNullTerminatedString();

        return new DefineFontNameTag(fontId, fontName, fontCopyright);
    }
}