namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// FrameLabel tag (TYPE = 43).
/// </summary>
public sealed class FrameLabelTag
{
    public const int TYPE = 43;

    public string Label { get; }
    public bool NamedAnchor { get; }

    public FrameLabelTag(string label, bool namedAnchor = false)
    {
        Label = label;
        NamedAnchor = namedAnchor;
    }

    /// <summary>
    /// Read a FrameLabel tag.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag's data.</param>
    public static FrameLabelTag Read(SwfReader reader, int end)
    {
        // Null-terminated string
        var label = reader.ReadNullTerminatedString();

        // Since SWF 6, an optional namedAnchor flag may follow
        var namedAnchor = (reader.Offset < end) && reader.ReadUi8() == 1;

        return new FrameLabelTag(label, namedAnchor);
    }
}