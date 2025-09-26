using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// SetBackgroundColor tag (type 9).
/// </summary>
public sealed class SetBackgroundColorTag
{
    public const int TYPE = 9;

    public Color Color { get; }

    public SetBackgroundColorTag(Color color)
    {
        Color = color;
    }

    /// <summary>
    /// Read a SetBackgroundColor tag from the reader.
    /// </summary>
    public static SetBackgroundColorTag Read(SwfReader reader)
    {
        return new SetBackgroundColorTag(Color.ReadRgb(reader));
    }
}
