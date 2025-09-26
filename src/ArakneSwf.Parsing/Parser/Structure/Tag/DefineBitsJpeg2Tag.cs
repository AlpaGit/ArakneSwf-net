using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineBitsJPEG2 tag (TYPE = 21).
/// Carries raw JPEG/PNG/GIF image bytes (no separate alpha stream).
/// </summary>
public sealed class DefineBitsJpeg2Tag : IDefineBitsJpegTag
{
    public const int TYPE = 21;

    public int CharacterId { get; }

    /// <inheritdoc />
    public ImageDataType Type { get; }

    /// <inheritdoc />
    public byte[] ImageData { get; }

    /// <inheritdoc />
    public byte[]? AlphaData { get; }  // always null for JPEG2

    public DefineBitsJpeg2Tag(int characterId, byte[] imageData)
    {
        CharacterId = characterId;
        ImageData = imageData;
        Type = ImageDataTypeExtensions.Resolve(imageData);
        AlphaData = null;
    }

    /// <summary>
    /// Read a DefineBitsJPEG2 tag from the stream.
    /// </summary>
    /// <param name="reader">SWF reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) for this tag's data.</param>
    public static DefineBitsJpeg2Tag Read(SwfReader reader, int end)
    {
        int characterId = reader.ReadUi16();
        byte[] imageData = reader.ReadBytesTo(end);
        return new DefineBitsJpeg2Tag(characterId, imageData);
    }
}
