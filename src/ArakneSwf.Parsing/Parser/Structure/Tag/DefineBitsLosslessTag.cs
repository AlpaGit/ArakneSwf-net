using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineBitsLossless (v1) / DefineBitsLossless2 (v2).
/// Carries paletted or true-color bitmap data, DEFLATE-compressed in-stream.
/// </summary>
public sealed class DefineBitsLosslessTag
{
    public const int TYPE_V1 = 20;
    public const int TYPE_V2 = 36;

    public const int FORMAT_8_BIT = 3;

    /** Only on v1 */
    public const int FORMAT_15_BIT = 4;

    /** Only on v1 */
    public const int FORMAT_24_BIT = 5;

    /** Only on v2 */
    public const int FORMAT_32_BIT = 5; // same numeric value as 24-bit in v1 (per SWF spec)

    public int Version { get; }
    public int CharacterId { get; }
    public int BitmapFormat { get; }

    /// <summary>SWF allows 0, but that would not be a valid image.</summary>
    public int BitmapWidth { get; }

    /// <summary>SWF allows 0, but that would not be a valid image.</summary>
    public int BitmapHeight { get; }

    /// <summary>
    /// For FORMAT_8_BIT: color table bytes (RGB for v1, RGBA for v2). Null otherwise.
    /// </summary>
    public byte[]? ColorTable { get; }

    /// <summary>
    /// Uncompressed pixel data.
    /// FORMAT_8_BIT: 1 byte per pixel, indexes into <see cref="ColorTable"/>.
    /// FORMAT_15_BIT: 2 bytes per pixel, 5R-5G-5B (first bit ignored).
    /// FORMAT_24_BIT: 3 bytes per pixel, 8R-8G-8B (first byte ignored by decoder).
    /// FORMAT_32_BIT: 4 bytes per pixel, 8A-8R-8G-8B.
    /// </summary>
    public byte[] PixelData { get; }

    /// <summary>Resolved image bitmap type (true-color vs paletted).</summary>
    public ImageBitmapType BitmapType => ImageBitmapTypeExtensions.FromTag(this);

    public DefineBitsLosslessTag(
        int     version,
        int     characterId,
        int     bitmapFormat,
        int     bitmapWidth,
        int     bitmapHeight,
        byte[]? colorTable,
        byte[]  pixelData)
    {
        Version = version;
        CharacterId = characterId;
        BitmapFormat = bitmapFormat;
        BitmapWidth = bitmapWidth;
        BitmapHeight = bitmapHeight;
        ColorTable = colorTable;
        PixelData = pixelData;
    }

    /// <summary>
    /// Read DefineBitsLossless (v1) or DefineBitsLossless2 (v2).
    /// </summary>
    /// <param name="reader">SWF reader.</param>
    /// <param name="version">1 for DefineBitsLossless, 2 for DefineBitsLossless2.</param>
    /// <param name="end">End byte offset (exclusive) of the tag payload.</param>
    public static DefineBitsLosslessTag Read(SwfReader reader, int version, int end)
    {
        int characterId = reader.ReadUi16();
        int bitmapFormat = reader.ReadUi8();
        int bitmapWidth = reader.ReadUi16();
        int bitmapHeight = reader.ReadUi16();

        if ((bitmapFormat < 3 || bitmapFormat > 5) && (reader.Errors & Errors.InvalidData) != 0)
        {
            throw new ParserInvalidDataException(
                $"Invalid bitmap format {bitmapFormat} for DefineBitsLossless tag (version {version})",
                reader.Offset
            );
        }

        byte[]? colorTable;
        byte[] pixelData;

        if (bitmapFormat == FORMAT_8_BIT)
        {
            int colors = reader.ReadUi8();
            // Read and inflate remaining data in one chunk
            var data = reader.ReadZLibTo(end);

            int colorSize = version > 1 ? 4 : 3; // v2: RGBA, v1: RGB
            int colorTableSize = colorSize * (colors + 1);

            // Guard against truncated data (be permissive as in PHP substr())
            int ctSize = Math.Min(colorTableSize, data.Length);
            colorTable = new byte[ctSize];
            Array.Copy(data, 0, colorTable, 0, ctSize);

            int pixelsLen = Math.Max(0, data.Length - ctSize);
            pixelData = new byte[pixelsLen];
            if (pixelsLen > 0)
                Array.Copy(data, ctSize, pixelData, 0, pixelsLen);
        }
        else
        {
            colorTable = null;
            pixelData = reader.ReadZLibTo(end);
        }

        return new DefineBitsLosslessTag(
            version: version,
            characterId: characterId,
            bitmapFormat: bitmapFormat,
            bitmapWidth: bitmapWidth,
            bitmapHeight: bitmapHeight,
            colorTable: colorTable,
            pixelData: pixelData
        );
    }
}

