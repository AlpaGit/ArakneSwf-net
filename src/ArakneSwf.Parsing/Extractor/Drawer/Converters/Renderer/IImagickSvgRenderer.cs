using ImageMagick;

namespace ArakneSwf.Parsing.Extractor.Drawer.Converters.Renderer;

/// <summary>
/// Perform SVG rendering and create a MagickImage.
/// </summary>
public interface IImagickSvgRenderer
{
    /// <summary>
    /// Parse the SVG string and return a MagickImage.
    /// </summary>
    /// <param name="svg">The SVG string to parse.</param>
    /// <param name="backgroundColor">Background color used when rasterizing (e.g., "transparent", "#ffffff").</param>
    /// <returns>The MagickImage representing the parsed SVG.</returns>
    MagickImage Open(string svg, string backgroundColor);

    /// <summary>
    /// Check if the current renderer is supported by the system.
    /// </summary>
    bool Supported();
}
