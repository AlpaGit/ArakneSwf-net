using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Image format for DefineBitsLossless / DefineBitsLossless2.
/// </summary>
public enum ImageBitmapType
{
    /// <summary>
    /// 8-bit paletted, opaque (version 1, FORMAT_8_BIT). Requires color table.
    /// </summary>
    Opaque8Bit,

    /// <summary>
    /// 15-bit RGB (5-5-5), opaque (version 1, FORMAT_15_BIT). No color table.
    /// </summary>
    Opaque15Bit,

    /// <summary>
    /// 24-bit true color, opaque (version 1, FORMAT_24_BIT). No color table.
    /// </summary>
    Opaque24Bit,

    /// <summary>
    /// 8-bit paletted with alpha (version 2, FORMAT_8_BIT). Requires color table.
    /// </summary>
    Transparent8Bit,

    /// <summary>
    /// 32-bit true color with alpha (version 2, FORMAT_32_BIT). No color table.
    /// </summary>
    Transparent32Bit
}

public static class ImageBitmapTypeExtensions
{
    /// <summary>
    /// Returns true for true-color formats (24-bit opaque or 32-bit with alpha).
    /// </summary>
    public static bool IsTrueColor(this ImageBitmapType type) =>
        type == ImageBitmapType.Opaque24Bit || type == ImageBitmapType.Transparent32Bit;

    /// <summary>
    /// Resolve the image bitmap type from a DefineBitsLossless tag.
    /// </summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if the tag version or bitmap format is not recognized.
    /// </exception>
    public static ImageBitmapType FromTag(DefineBitsLosslessTag tag)
    {
        // Expects:
        // - tag.Version: 1 for DefineBitsLossless, 2 for DefineBitsLossless2
        // - tag.BitmapFormat: FORMAT_8_BIT / FORMAT_15_BIT / FORMAT_24_BIT / FORMAT_32_BIT
        return tag.Version switch
        {
            1 => tag.BitmapFormat switch
            {
                DefineBitsLosslessTag.FORMAT_8_BIT  => ImageBitmapType.Opaque8Bit,
                DefineBitsLosslessTag.FORMAT_15_BIT => ImageBitmapType.Opaque15Bit,
                DefineBitsLosslessTag.FORMAT_24_BIT => ImageBitmapType.Opaque24Bit,
                _ => throw new System.ArgumentOutOfRangeException(nameof(tag.BitmapFormat),
                                                                  $"Unknown bitmap format for version 1: {tag.BitmapFormat}")
            },

            2 => tag.BitmapFormat switch
            {
                DefineBitsLosslessTag.FORMAT_8_BIT  => ImageBitmapType.Transparent8Bit,
                DefineBitsLosslessTag.FORMAT_32_BIT => ImageBitmapType.Transparent32Bit,
                _ => throw new System.ArgumentOutOfRangeException(nameof(tag.BitmapFormat),
                                                                  $"Unknown bitmap format for version 2: {tag.BitmapFormat}")
            },

            _ => throw new System.ArgumentOutOfRangeException(nameof(tag.Version),
                                                              $"Unknown version: {tag.Version}")
        };
    }
}