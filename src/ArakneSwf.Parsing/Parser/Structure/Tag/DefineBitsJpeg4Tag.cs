using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineBitsJPEG4 tag (TYPE = 90).
/// Like JPEG3 but adds a deblock parameter; alpha stream is ZLIB-compressed.
/// </summary>
public sealed class DefineBitsJpeg4Tag : IDefineBitsJpegTag
{
    public const int TYPE = 90;

    public int CharacterId { get; }
    public int DeblockParam { get; }

    /// <inheritdoc />
    public ImageDataType Type { get; }

    /// <inheritdoc />
    public byte[] ImageData { get; }

    /// <inheritdoc />
    public byte[]? AlphaData { get; }

    public DefineBitsJpeg4Tag(int characterId, int deblockParam, byte[] imageData, byte[]? alphaData)
    {
        CharacterId = characterId;
        DeblockParam = deblockParam;
        ImageData = imageData;
        Type = ImageDataTypeExtensions.Resolve(imageData);
        AlphaData = alphaData;
    }

    /// <summary>
    /// Read a DefineBitsJPEG4 tag from the stream.
    /// </summary>
    /// <param name="reader">SWF reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) for this tag's data.</param>
    public static DefineBitsJpeg4Tag Read(SwfReader reader, int end)
    {
        int characterId = reader.ReadUi16();
        var alphaDataOffset = (int)reader.ReadUi32(); // length of image data block
        int deblockParam = reader.ReadUi16();

        var imageData = reader.ReadBytes(alphaDataOffset);

        byte[]? alphaData = null;
        if (end > reader.Offset)
        {
            var inflated = reader.ReadZLibTo(end); // uncompressed alpha (one byte per pixel)
            alphaData = (inflated != null && inflated.Length > 0) ? inflated : null;
        }

        return new DefineBitsJpeg4Tag(characterId, deblockParam, imageData, alphaData);
    }
}