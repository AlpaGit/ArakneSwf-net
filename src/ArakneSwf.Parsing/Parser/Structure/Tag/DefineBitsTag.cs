namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineBits tag (TYPE = 6): raw JPEG table-less image bytes.
/// </summary>
public sealed class DefineBitsTag
{
    public const int TYPE = 6;

    public int CharacterId { get; }
    public byte[] ImageData { get; }

    public DefineBitsTag(int characterId, byte[] imageData)
    {
        CharacterId = characterId;
        ImageData = imageData;
    }

    /// <summary>
    /// Read a DefineBitsTag from the reader.
    /// </summary>
    /// <param name="reader">SWF reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag's data.</param>
    public static DefineBitsTag Read(SwfReader reader, int end)
    {
        int characterId = reader.ReadUi16();
        var imageData = reader.ReadBytesTo(end);
        return new DefineBitsTag(characterId, imageData);
    }
}
