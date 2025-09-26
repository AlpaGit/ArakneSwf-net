namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// Marks the SWF as created by rfxswf. Can be ignored.
/// Note: not documented in the official SWF spec.
/// </summary>
public sealed class ReflexTag
{
    public const int TYPE = 777;

    public byte[] Name { get; }

    public ReflexTag(byte[] name)
    {
        Name = name;
    }

    /// <summary>
    /// Read a Reflex tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset of the tag body.</param>
    public static ReflexTag Read(SwfReader reader, int end)
    {
        return new ReflexTag(reader.ReadBytesTo(end));
    }
}