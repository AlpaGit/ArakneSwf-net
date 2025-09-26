using System.Globalization;
using System.Text;

namespace ArakneSwf.Parsing.Extractor.Drawer.Converters;

/// <summary>Enum of supported image formats.</summary>
public enum ImageFormat
{
    Svg,
    Png,
    Jpeg,
    Gif,
    Webp
}

public static class ImageFormatExtensions
{
    /// <summary>
    /// Convert the drawable to the specified format.
    /// For raster formats returns the encoded bytes; for SVG returns UTF-8 bytes of the SVG XML.
    /// <para>
    /// <paramref name="options"/> corresponds to the PHP <c>array&lt;string, string|bool&gt;</c>.
    /// </para>
    /// </summary>
    public static byte[] DoConvert(
        this ImageFormat              format,
        Converter                     converter,
        IDrawable                     drawable,
        int                           frame   = 0,
        IDictionary<string, object?>? options = null)
    {
        return format switch
        {
            ImageFormat.Svg  => Encoding.UTF8.GetBytes(converter.ToSvg(drawable, frame)),
            ImageFormat.Png  => converter.ToPng(drawable, frame, ToPngOptions(options)),
            ImageFormat.Jpeg => converter.ToJpeg(drawable, frame, ToJpegOptions(options)),
            ImageFormat.Gif  => converter.ToGif(drawable, frame, ToGifOptions(options)),
            ImageFormat.Webp => converter.ToWebp(drawable, frame, ToWebpOptions(options)),
            _                => throw new NotSupportedException($"Unsupported format: {format}")
        };
    }

    /// <summary>
    /// Convenience helper if you specifically want the SVG as a string.
    /// Throws if the format is not SVG.
    /// </summary>
    public static string ConvertToSvgString(
        this ImageFormat format,
        Converter        converter,
        IDrawable        drawable,
        int              frame = 0)
    {
        if (format != ImageFormat.Svg)
            throw new InvalidOperationException("ConvertToSvgString can only be used with ImageFormat.Svg.");
        return converter.ToSvg(drawable, frame);
    }

    /// <summary>
    /// Render the character as an animated image (GIF or WebP).
    /// </summary>
    public static byte[] Animation(
        this ImageFormat              format,
        Converter                     converter,
        IDrawable                     drawable,
        int                           fps,
        bool                          recursive = false,
        IDictionary<string, object?>? options   = null)
    {
        return format switch
        {
            ImageFormat.Gif  => converter.ToAnimatedGif(drawable, fps, recursive, ToGifOptions(options)),
            ImageFormat.Webp => converter.ToAnimatedWebp(drawable, fps, recursive, ToWebpOptions(options)),
            _                => throw new InvalidOperationException("Animation not supported for this format")
        };
    }

    /// <summary>Get the file extension for this image format.</summary>
    public static string Extension(this ImageFormat format) => format switch
    {
        ImageFormat.Svg  => "svg",
        ImageFormat.Png  => "png",
        ImageFormat.Jpeg => "jpeg",
        ImageFormat.Gif  => "gif",
        ImageFormat.Webp => "webp",
        _                => throw new NotSupportedException($"Unsupported format: {format}")
    };

    // ---------- option mappers (dictionary -> typed options) ----------

    private static PngOptions? ToPngOptions(IDictionary<string, object?>? dict)
        => dict is null
            ? null
            : new PngOptions
            {
                Compression = GetInt(dict, "compression"),
                Format = GetString(dict, "format"),
                BitDepth = GetInt(dict, "bit-depth"),
            };

    private static JpegOptions? ToJpegOptions(IDictionary<string, object?>? dict)
        => dict is null
            ? null
            : new JpegOptions
            {
                Quality = GetInt(dict, "quality"),
                Sampling = GetString(dict, "sampling"),
                Size = GetString(dict, "size"),
            };

    private static GifOptions? ToGifOptions(IDictionary<string, object?>? dict)
        => dict is null
            ? null
            : new GifOptions
            {
                Loop = GetInt(dict, "loop"),
                Optimize = GetString(dict, "optimize"),
            };

    private static WebpOptions? ToWebpOptions(IDictionary<string, object?>? dict)
        => dict is null
            ? null
            : new WebpOptions
            {
                Lossless = GetBool(dict, "lossless"),
                Compression = GetInt(dict, "compression"),
                Quality = GetInt(dict, "quality"),
            };

    // ---------- helpers ----------

    private static int? GetInt(IDictionary<string, object?> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v) || v is null) return null;

        return v switch
        {
            int i     => i,
            long l    => checked((int)l),
            float f   => (int)f,
            double d  => (int)d,
            decimal m => (int)m,
            string s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i2)
                ? i2
                : (int?)null,
            _ => null
        };
    }

    private static bool? GetBool(IDictionary<string, object?> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v) || v is null) return null;

        return v switch
        {
            bool b   => b,
            string s => bool.TryParse(s, out var b2) ? b2 : (bool?)null,
            _        => null
        };
    }

    private static string? GetString(IDictionary<string, object?> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v) || v is null) return null;
        return v switch
        {
            string s => s,
            bool b   => b ? "true" : "false",
            _        => Convert.ToString(v, CultureInfo.InvariantCulture)
        };
    }
}
