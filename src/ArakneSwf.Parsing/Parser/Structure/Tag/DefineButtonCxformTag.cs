using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineButtonCxform tag (TYPE = 23).
/// </summary>
public sealed class DefineButtonCxformTag
{
    public const int TYPE = 23;

    public int ButtonId { get; }
    public ColorTransform ColorTransform { get; }

    public DefineButtonCxformTag(int buttonId, ColorTransform colorTransform)
    {
        ButtonId = buttonId;
        ColorTransform = colorTransform;
    }

    /// <summary>
    /// Read a DefineButtonCxform tag from the stream.
    /// </summary>
    public static DefineButtonCxformTag Read(SwfReader reader)
    {
        int buttonId = reader.ReadUi16();
        var cx = ColorTransform.Read(reader, withAlpha: false);
        return new DefineButtonCxformTag(buttonId, cx);
    }
}
