using System.Globalization;
using System.Xml.Linq;
using ArakneSwf.Parsing.Extractor.Drawer.Converters.Renderer;
using ArakneSwf.Parsing.Extractor.Drawer.Svg;
using ImageMagick;

namespace ArakneSwf.Parsing.Extractor.Drawer.Converters;

public sealed class Converter
{
    private readonly IImageResizer? _resizer;
    private readonly string _backgroundColor;
    private readonly IImagickSvgRenderer? _svgRenderer;

    /// <param name="resizer">Optional resizer to scale the final SVG width/height.</param>
    /// <param name="backgroundColor">Background color used when rasterizing (e.g., "transparent", "#ffffff").</param>
    /// <param name="svgRenderer">Optional SVG renderer implementation for Magick.NET.</param>
    public Converter(
        IImageResizer?       resizer         = null,
        string               backgroundColor = "transparent",
        IImagickSvgRenderer? svgRenderer     = null)
    {
        _resizer = resizer;
        _backgroundColor = backgroundColor;
        _svgRenderer = svgRenderer;
    }

    /// <summary>
    /// Convert the object to SVG, and apply the resizer if needed.
    /// </summary>
    public string ToSvg(IDrawable drawable, int frame = 0)
    {
        // draw → svg
        var canvas = new SvgCanvas(drawable.Bounds());
        drawable.Draw(canvas, frame);
        var svg = canvas.Render();

        if (_resizer == null)
            return svg;

        // parse and adjust width/height + viewBox
        var xml = XDocument.Parse(svg);
        var root = xml.Root ?? throw new InvalidOperationException("Invalid SVG output.");

        // Read width/height if present; else compute from bounds (twips → px /20)
        var width = ReadSizeAttr(root, "width") ?? (drawable.Bounds().Width() / 20.0);
        var height = ReadSizeAttr(root, "height") ?? (drawable.Bounds().Height() / 20.0);

        var (newW, newH) = _resizer.Scale(width, height);

        root.SetAttributeValue("width", newW.ToString(CultureInfo.InvariantCulture) + "px");
        root.SetAttributeValue("height", newH.ToString(CultureInfo.InvariantCulture) + "px");
        root.SetAttributeValue("viewBox",
                               $"0 0 {width.ToString(CultureInfo.InvariantCulture)} {height.ToString(CultureInfo.InvariantCulture)}");

        using var sw = new StringWriter();
        xml.Save(sw, SaveOptions.DisableFormatting);
        return sw.ToString();

        static double? ReadSizeAttr(XElement el, string name)
        {
            var raw = (string?)el.Attribute(name);
            if (string.IsNullOrEmpty(raw)) return null;

            // strip optional "px"
            raw = raw.Trim();
            if (raw.EndsWith("px", StringComparison.OrdinalIgnoreCase))
                raw = raw[..^2];

            return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                ? v
                : null;
        }
    }

    /// <summary>Render the drawable to PNG format.</summary>
    public byte[] ToPng(IDrawable drawable, int frame = 0, PngOptions? options = null)
    {
        using var img = ToMagick(drawable, frame);
        img.Format = MagickFormat.Png;
        ApplyPngOptions(img, options);
        return img.ToByteArray();
    }

    /// <summary>
    /// Render the drawable to GIF format (single frame).
    /// Note: does not generate animated GIFs. Use <see cref="ToAnimatedGif"/> for animation.
    /// </summary>
    public byte[] ToGif(IDrawable drawable, int frame = 0, GifOptions? options = null)
    {
        using var img = ToMagick(drawable, frame);
        img.Format = MagickFormat.Gif;
        ApplyGifOptions(img, options);
        return img.ToByteArray();
    }

    /// <summary>Render all frames of the drawable as an animated GIF image.</summary>
    public byte[] ToAnimatedGif(IDrawable drawable, int fps = 24, bool recursive = false, GifOptions? options = null)
    {
        using var anim = new MagickImageCollection();
        return RenderAnimatedImage(anim,
                                   MagickFormat.Gif,
                                   drawable,
                                   fps,
                                   recursive,
                                   img => ApplyGifOptions(img, options));
    }

    /// <summary>Render the drawable to WebP format.</summary>
    public byte[] ToWebp(IDrawable drawable, int frame = 0, WebpOptions? options = null)
    {
        using var img = ToMagick(drawable, frame);
        img.Format = MagickFormat.WebP;
        ApplyWebpOptions(img, options);
        return img.ToByteArray();
    }

    /// <summary>Render all frames of the drawable as an animated WebP image.</summary>
    public byte[] ToAnimatedWebp(IDrawable drawable, int fps = 24, bool recursive = false, WebpOptions? options = null)
    {
        using var anim = new MagickImageCollection();
        return RenderAnimatedImage(anim,
                                   MagickFormat.WebP,
                                   drawable,
                                   fps,
                                   recursive,
                                   img => ApplyWebpOptions(img, options));
    }

    /// <summary>
    /// Render the drawable to JPEG format.
    /// Because transparency is not supported in JPEG, define a non-transparent background color in the constructor.
    /// </summary>
    public byte[] ToJpeg(IDrawable drawable, int frame = 0, JpegOptions? options = null)
    {
        using var img = ToMagick(drawable, frame);
        img.Format = MagickFormat.Jpeg;
        ApplyJpegOptions(img, options);
        return img.ToByteArray();
    }

    // --- internals ---

    private MagickImage ToMagick(IDrawable drawable, int frame = 0)
    {
        var svg = ToSvg(drawable, frame);
        var renderer = _svgRenderer ?? ImagickSvgRendererResolver.Get();

        // Implementations may choose density/background, etc. here.
        // Expectation: Open returns a single-frame MagickImage.
        var img = renderer.Open(svg, _backgroundColor);
        if (img is null) throw new InvalidOperationException("SVG renderer failed to open image.");
        return img;
    }

    private byte[] RenderAnimatedImage(
        MagickImageCollection target,
        MagickFormat          format,
        IDrawable             drawable,
        int                   fps,
        bool                  recursive,
        Action<MagickImage>?  optionsConfigurator = null)
    {
        var count = drawable.FramesCount(recursive);
        var delay = Math.Max((int)Math.Round(100.0 / Math.Max(fps, 1)), 1); // centiseconds/frame

        for (var frame = 0; frame < count; frame++)
        {
            using var img = ToMagick(drawable, frame);
            img.Format = format;

            // Animation timing & disposal (2 = Background)
            img.AnimationDelay = (uint)delay;
            img.GifDisposeMethod = GifDisposeMethod.Background;

            optionsConfigurator?.Invoke(img);
            target.Add(img.Clone()); // add a clone; we'll dispose the original
        }

        // Some options (e.g., loop) may apply on the collection as a whole. Re-apply on last frame if needed by your pipeline.
        // Write to a byte array:
        return target.ToByteArray(format);
    }

    private static void ApplyPngOptions(MagickImage img, PngOptions? options)
    {
        if (options is null) return;

        // Compression level
        if (options.Compression.HasValue)
        {
            img.Settings.SetDefine(MagickFormat.Png,
                                   "compression-level",
                                   options.Compression.Value.ToString(CultureInfo.InvariantCulture));
        }

        // Format (e.g., "rgba", "grayscale") – passed as a write define when needed.
        if (!string.IsNullOrEmpty(options.Format))
        {
            img.Settings.SetDefine(MagickFormat.Png, "format", options.Format!);
        }

        if (options.BitDepth.HasValue)
        {
            img.Settings.SetDefine(MagickFormat.Png,
                                   "bit-depth",
                                   options.BitDepth.Value.ToString(CultureInfo.InvariantCulture));
            img.Depth = (uint)options.BitDepth.Value;
        }
    }

    private static void ApplyWebpOptions(MagickImage img, WebpOptions? options)
    {
        if (options is null) return;

        if (options.Lossless == true)
        {
            img.Settings.SetDefine(MagickFormat.WebP, "lossless", "true");
        }

        if (options.Quality.HasValue)
        {
            img.Quality = (uint)Math.Clamp(options.Quality.Value, 1, 100);
        }

        if (options.Compression.HasValue)
        {
            // "method" is 0..6 in libwebp: 0=fast, 6=slow/best
            img.Settings.SetDefine(MagickFormat.WebP,
                                   "method",
                                   options.Compression.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static void ApplyJpegOptions(MagickImage img, JpegOptions? options)
    {
        if (options is null) return;

        if (options.Quality.HasValue)
            img.Quality = (uint)Math.Clamp(options.Quality.Value, 1, 100);

        if (!string.IsNullOrEmpty(options.Sampling))
        {
            // Map to JPEG sampling-factor define (e.g. 4:4:4, 4:2:2, 4:2:0, 4:1:1)
            var factor = options.Sampling switch
            {
                "444" => "4:4:4",
                "422" => "4:2:2",
                "420" => "4:2:0",
                "411" => "4:1:1",
                _     => throw new ArgumentException($"Invalid sampling factor: {options.Sampling}")
            };

            img.Settings.SetDefine(MagickFormat.Jpeg, "sampling-factor", factor);
        }

        if (!string.IsNullOrEmpty(options.Size))
        {
            // Maximum output size, e.g., "200kb"
            img.Settings.SetDefine(MagickFormat.Jpeg, "extent", options.Size!);
        }
    }

    private static void ApplyGifOptions(MagickImage img, GifOptions? options)
    {
        if (options is null) return;

        if (options.Loop.HasValue)
            img.AnimationIterations = (uint)options.Loop.Value;

        if (!string.IsNullOrEmpty(options.Optimize))
        {
            // Pass-through write define. For advanced optimization, you could run:
            // using var col = new MagickImageCollection(img.Clone()); col.OptimizeTransparency();
            img.Settings.SetDefine(MagickFormat.Gif, "optimize", options.Optimize!);
        }
    }
}

public sealed class PngOptions
{
    public int? Compression { get; init; } // png:compression-level
    public string? Format { get; init; }   // png:format
    public int? BitDepth { get; init; }    // png:bit-depth
}

public sealed class WebpOptions
{
    public bool?  Lossless    { get; init; } // webp:lossless
    public int?   Compression { get; init; } // webp:method (0..6)
    public int?   Quality     { get; init; } // image quality (1..100)
}

public sealed class JpegOptions
{
    public int?    Quality  { get; init; } // compression quality (1..100)
    public string? Sampling { get; init; } // "444"|"422"|"420"|"411"
    public string? Size     { get; init; } // jpeg:extent, e.g. "200kb"
}

public sealed class GifOptions
{
    public int?    Loop     { get; init; } // animation iterations
    public string? Optimize { get; init; } // gif:optimize define
}
