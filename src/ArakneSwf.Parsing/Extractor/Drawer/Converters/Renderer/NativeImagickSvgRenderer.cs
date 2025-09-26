using System.Text;
using ImageMagick;

namespace ArakneSwf.Parsing.Extractor.Drawer.Converters.Renderer;

/// <summary>
/// Directly parse the SVG string using Magick.NET (requires an SVG delegate available in ImageMagick).
/// </summary>
public sealed class NativeImagickSvgRenderer : IImagickSvgRenderer
{
    public MagickImage Open(string svg, string backgroundColor)
    {
        if (svg is null) throw new ArgumentNullException(nameof(svg));

        var settings = new MagickReadSettings
        {
            BackgroundColor = new MagickColor(backgroundColor),
            Format = MagickFormat.Svg
        };
        
        // Read from UTF-8 memory stream as SVG format
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var img = new MagickImage();
        img.Read(ms, settings);

        // Keep background color on the image for later flattening if desired
        img.BackgroundColor = new MagickColor(backgroundColor);
        return img;
    }

    public bool Supported()
    {
        // Heuristic: try to load a minimal SVG. If a delegate is missing, this will throw.
        const string tinySvg = "<svg xmlns='http://www.w3.org/2000/svg' width='1' height='1'/>";

        try
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(tinySvg));
            using var img = new MagickImage();
            img.Read(ms, MagickFormat.Svg);
            return true;
        }
        catch (MagickException)
        {
            return false;
        }
    }
}