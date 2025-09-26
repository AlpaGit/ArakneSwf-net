using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineBitsJPEG3 tag (TYPE = 35).
/// Carries raw image bytes (JPEG/PNG/GIF89a) and, for JPEG only, an optional
/// separate ZLIB-compressed alpha stream (uncompressed to one byte per pixel).
/// </summary>
public sealed class DefineBitsJpeg3Tag : IDefineBitsJpegTag
{
    public const int TYPE = 35;

    public int CharacterId { get; }

    /// <inheritdoc />
    public ImageDataType Type { get; }

    /// <inheritdoc />
    public byte[] ImageData { get; }

    /// <inheritdoc />
    public byte[]? AlphaData { get; }

    public DefineBitsJpeg3Tag(int characterId, byte[] imageData, byte[]? alphaData)
    {
        CharacterId = characterId;
        ImageData = imageData;
        Type = ImageDataTypeExtensions.Resolve(imageData);
        AlphaData = alphaData;
    }

    /// <summary>
    /// Read a DefineBitsJPEG3 tag from the stream.
    /// </summary>
    /// <param name="reader">SWF reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) for this tag's data.</param>
    public static DefineBitsJpeg3Tag Read(SwfReader reader, int end)
    {
        int characterId = reader.ReadUi16();

        // Image data length (UI32), then raw bytes
        var imageDataLen = reader.ReadUi32();
        byte[] imageData = reader.ReadBytes((int)imageDataLen);

        byte[]? alphaData = null;

        // If there is remaining data, it's a ZLIB-compressed alpha layer.
        if (reader.Offset < end)
        {
            // Read and inflate the remaining bytes to end
            // (SwfReader.ReadZLibTo should return the uncompressed bytes, or empty if invalid & errors masked)
            var inflated = reader.ReadZLibTo(end);
            alphaData = (inflated != null && inflated.Length > 0) ? inflated : null;
        }

        return new DefineBitsJpeg3Tag(characterId, imageData, alphaData);
    }
}