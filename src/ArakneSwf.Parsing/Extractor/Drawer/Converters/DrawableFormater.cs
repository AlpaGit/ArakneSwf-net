namespace ArakneSwf.Parsing.Extractor.Drawer.Converters;

/// <summary>
/// Formats a drawable to a specific image format.
/// </summary>
public sealed class DrawableFormater
{
    private Converter? _converter;

    public ImageFormat Format { get; }
    public IImageResizer? Size { get; }
    public IReadOnlyDictionary<string, object?> Options { get; }

    /// <param name="format">Target image format.</param>
    /// <param name="size">Optional resizer to apply.</param>
    /// <param name="options">Additional conversion options (keys like "quality", "compression", etc.).</param>
    public DrawableFormater(
        ImageFormat                           format,
        IImageResizer?                        size    = null,
        IReadOnlyDictionary<string, object?>? options = null)
    {
        Format = format;
        Size = size;
        Options = options ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Render the drawable to the specified image format.
    /// Returns encoded bytes (SVG returned as UTF-8 bytes).
    /// </summary>
    public byte[] FormatDrawable(IDrawable drawable, int frame = 0)
    {
        _converter ??= new Converter(Size);
        return Format.DoConvert(_converter, drawable, frame, Options as IDictionary<string, object?>);
    }

    /// <summary>
    /// Get the file extension for this image format.
    /// </summary>
    public string Extension() => Format.Extension();

    // Optional: if you specifically want SVG as a string (throws if not SVG).
    public string FormatSvgString(IDrawable drawable, int frame = 0)
        => Format.ConvertToSvgString(_converter ??= new Converter(Size), drawable, frame);
}