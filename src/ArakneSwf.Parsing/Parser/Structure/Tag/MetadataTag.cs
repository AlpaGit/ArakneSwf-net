namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// Metadata tag (TYPE = 77).
/// </summary>
public sealed class MetadataTag
{
    public const int TYPE = 77;

    public string Metadata { get; }

    public MetadataTag(string metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Read a Metadata tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    public static MetadataTag Read(SwfReader reader)
    {
        return new MetadataTag(reader.ReadNullTerminatedString());
    }
}
