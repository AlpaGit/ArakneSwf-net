using System.Globalization;

namespace ArakneSwf.Parsing.Extractor.Drawer.Converters.Renderer;

/// <summary>
/// Parse the SVG string using Inkscape to render as PNG, before passing it to Magick.NET.
/// </summary>
public sealed class InkscapeImagickSvgRenderer : AbstractCommandImagickSvgRenderer
{
    public InkscapeImagickSvgRenderer(string command = "inkscape")
        : base(command) { }

    protected override string BuildCommand(string command, string backgroundColor)
    {
        var (bgHex, bgOpacity) = ParseBackgroundColor(backgroundColor);

        // inkscape --pipe --export-type=png -b <bgColor> -y <opacity>
        return
            $"{EscapeShellArg(command)} --pipe --export-type=png -b {EscapeShellArg(bgHex)} -y {bgOpacity.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Parse CSS-like color strings to (hexColor, opacity).
    /// Supported: "", "none", "transparent", "#rrggbb", "rgba(r,g,b,a)", "rgb(r,g,b)", named colors (passed-through, opacity=1).
    /// </summary>
    private static (string hexColor, double opacity) ParseBackgroundColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return ("#000000", 0.0);

        color = color.Trim();

        if (string.Equals(color, "none", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(color, "transparent", StringComparison.OrdinalIgnoreCase))
        {
            return ("#000000", 0.0);
        }

        if (color[0] == '#')
            return (color, 1.0);

        var lower = color.ToLowerInvariant();

        if (lower.StartsWith("rgba(") && lower.EndsWith(")"))
        {
            var inner = lower.Substring(5, lower.Length - 6); // contents between rgba( ...)
            var parts = inner.Split(',', 4, StringSplitOptions.TrimEntries);
            if (parts.Length != 4)
                throw new ArgumentException("Invalid RGBA color format", nameof(color));

            int r = ClampByte(ParseInt(parts[0]));
            int g = ClampByte(ParseInt(parts[1]));
            int b = ClampByte(ParseInt(parts[2]));
            double a = Clamp01(ParseDouble(parts[3]));

            return ($"#{r:X2}{g:X2}{b:X2}", a);
        }

        if (lower.StartsWith("rgb(") && lower.EndsWith(")"))
        {
            var inner = lower.Substring(4, lower.Length - 5); // contents between rgb( ...)
            var parts = inner.Split(',', 3, StringSplitOptions.TrimEntries);
            if (parts.Length != 3)
                throw new ArgumentException("Invalid RGB color format", nameof(color));

            int r = ClampByte(ParseInt(parts[0]));
            int g = ClampByte(ParseInt(parts[1]));
            int b = ClampByte(ParseInt(parts[2]));

            return ($"#{r:X2}{g:X2}{b:X2}", 1.0);
        }

        // Named color or other CSS value; pass through with full opacity.
        return (color, 1.0);

        static int ParseInt(string s) =>
            int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
                ? v
                : throw new ArgumentException("Invalid integer in color");

        static double ParseDouble(string s) =>
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                ? v
                : throw new ArgumentException("Invalid float in color");

        static int    ClampByte(int  v) => Math.Min(255, Math.Max(0, v));
        static double Clamp01(double v) => Math.Min(1.0, Math.Max(0.0, v));
    }
}