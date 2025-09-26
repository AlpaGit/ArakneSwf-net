namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineBinaryData tag (TYPE = 87).
/// </summary>
public sealed class DefineBinaryDataTag
{
    public const int TYPE = 87;

    public int Tag { get; }
    public byte[] Data { get; }

    public DefineBinaryDataTag(int tag, byte[] data)
    {
        Tag = tag;
        Data = data;
    }

    /// <summary>
    /// Read a DefineBinaryData tag from the SWF reader.
    /// </summary>
    /// <param name="reader">SWF reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of the tag data.</param>
    public static DefineBinaryDataTag Read(SwfReader reader, int end)
    {
        int tag = reader.ReadUi16();
        reader.SkipBytes(4); // reserved, must be 0

        // Read remaining bytes up to 'end'
        byte[] data = reader.ReadBytesTo(end);

        return new DefineBinaryDataTag(tag, data);
    }
}
