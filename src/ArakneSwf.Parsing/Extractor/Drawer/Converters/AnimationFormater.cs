namespace ArakneSwf.Parsing.Extractor.Drawer.Converters;

/// <summary>
/// Formats a drawable as an animated image.
/// </summary>
public sealed class AnimationFormater
{
    private Converter? _converter;

    public ImageFormat Format { get; }
    public IImageResizer? Size { get; }
    public IReadOnlyDictionary<string, object?> Options { get; }

    /// <param name="format">Target animated image format (Gif or Webp).</param>
    /// <param name="size">Optional resizer to apply.</param>
    /// <param name="options">Additional conversion options.</param>
    public AnimationFormater(
        ImageFormat                           format,
        IImageResizer?                        size    = null,
        IReadOnlyDictionary<string, object?>? options = null)
    {
        Format = format;
        Size = size;
        Options = options ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Render the drawable as an animated image.
    /// </summary>
    /// <param name="drawable">The drawable to render.</param>
    /// <param name="fps">Frame rate of the animation (positive).</param>
    /// <param name="recursive">If true, count frames recursively in children.</param>
    /// <returns>Encoded image bytes.</returns>
    public byte[] DoFormat(IDrawable drawable, int fps, bool recursive = false)
    {
        _converter ??= new Converter(Size);
        return Format.Animation(_converter, drawable, fps, recursive, Options as IDictionary<string, object?>);
    }

    /// <summary>Get the file extension for this image format.</summary>
    public string Extension() => Format.Extension();
}