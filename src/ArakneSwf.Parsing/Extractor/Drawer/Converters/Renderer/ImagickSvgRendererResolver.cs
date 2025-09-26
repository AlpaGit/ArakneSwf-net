namespace ArakneSwf.Parsing.Extractor.Drawer.Converters.Renderer;

public static class ImagickSvgRendererResolver
{
    private static readonly Type[] Implementations =
    [
        typeof(NativeImagickSvgRenderer),
        typeof(RsvgImagickSvgRenderer),
        typeof(InkscapeImagickSvgRenderer)
    ];

    private static IImagickSvgRenderer? _instance;

    /// <summary>
    /// Get the first supported SVG renderer available on the system.
    /// </summary>
    public static IImagickSvgRenderer Get()
    {
        if (_instance is not null)
            return _instance;

        foreach (var type in Implementations)
        {
            IImagickSvgRenderer? instance = null;
            try
            {
                instance = CreateInstance(type);
            }
            catch
            {
                // Ignore ctor failures and try next implementation
            }

            if (instance is null)
                continue;

            try
            {
                if (instance.Supported())
                    return _instance = instance;
            }
            catch
            {
                // If Supported() throws, ignore and try next implementation
            }
        }

        throw new InvalidOperationException(
            "No supported SVG renderer found. Please install Inkscape, rsvg-convert, or enable an SVG delegate in ImageMagick.");
    }

    private static IImagickSvgRenderer CreateInstance(Type type)
    {
        switch (type)
        {
            case not null when type == typeof(NativeImagickSvgRenderer):
                return new NativeImagickSvgRenderer();
            case not null when type == typeof(RsvgImagickSvgRenderer):
                return new RsvgImagickSvgRenderer();
            case not null when type == typeof(InkscapeImagickSvgRenderer):
                return new InkscapeImagickSvgRenderer();
            default:
                throw new ArgumentException($"Type {type.FullName} is not a recognized IImagickSvgRenderer implementation.");
        }
    }

    /// <summary>
    /// Optionally override the detected renderer (useful for tests/DI).
    /// </summary>
    public static void Set(IImagickSvgRenderer renderer)
    {
        _instance = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    /// <summary>
    /// Clears the cached instance so detection will run again on next Get().
    /// </summary>
    public static void Reset() => _instance = null;
}