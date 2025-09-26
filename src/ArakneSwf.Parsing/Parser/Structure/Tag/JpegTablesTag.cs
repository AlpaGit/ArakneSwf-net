namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// JPEGTables tag (TYPE = 8).
/// </summary>
public sealed class JpegTablesTag
{
    public const int TYPE = 8;

    public byte[] Data { get; }

    public JpegTablesTag(byte[] data)
    {
        Data = data;
    }

    /// <summary>
    /// Read a JPEGTables tag from the reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag's data.</param>
    public static JpegTablesTag Read(SwfReader reader, int end)
    {
        return new JpegTablesTag(reader.ReadBytesTo(end));
    }
}
