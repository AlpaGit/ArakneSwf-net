namespace ArakneSwf.Parsing.Extractor.Drawer.Converters.Renderer;

/// <summary>
/// Parse the SVG string using `rsvg-convert` to render as PNG, then load into Magick.NET.
/// </summary>
public sealed class RsvgImagickSvgRenderer : AbstractCommandImagickSvgRenderer
{
    public RsvgImagickSvgRenderer(string command = "rsvg-convert")
        : base(command)
    {
    }

    protected override string BuildCommand(string command, string backgroundColor)
    {
        // rsvg-convert -f png -b <bgColor>
        return $"{EscapeShellArg(command)} -f png -b {EscapeShellArg(backgroundColor)}";
    }
}
