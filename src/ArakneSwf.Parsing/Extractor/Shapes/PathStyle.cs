using ArakneSwf.Parsing.Extractor.Shapes.FillTypes;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Define the drawing style of a path.
/// This style is common for line and fill paths and is also used as a key
/// to allow merging paths with the same style.
/// </summary>
public sealed class PathStyle
{
    private readonly string _hash;

    /// <param name="fill">
    /// The fill style and color of the current path.
    /// If null, the path should not be filled.
    /// </param>
    /// <param name="lineColor">
    /// The line color of the current path.
    /// If null, the path should not be stroked, unless <see cref="LineFill"/> is set.
    /// </param>
    /// <param name="lineFill">
    /// Draw the stroke line with a fill style. Unlike <see cref="Fill"/>,
    /// the fill will be applied on the stroke line within its width,
    /// instead of filling the polygon.
    /// </param>
    /// <param name="lineWidth">
    /// The width of the line in twips (1/20 px). Should be set only if
    /// <see cref="LineColor"/> or <see cref="LineFill"/> is set.
    /// </param>
    /// <param name="reverse">
    /// Should edges be added in reverse order? True for style0 fill paths.
    /// Not used in the hash; only applied while building paths.
    /// </param>
    public PathStyle(
        IFillType? fill      = null,
        Color?     lineColor = null,
        IFillType? lineFill  = null,
        int        lineWidth = 0,
        bool       reverse   = false)
    {
        Fill = fill;
        LineColor = lineColor;
        LineFill = lineFill;
        LineWidth = lineWidth;
        Reverse = reverse;

        _hash = (Fill?.Hash() ?? string.Empty) +
                (LineFill?.Hash() ?? string.Empty) +
                "-" +
                ColorHash(LineColor) +
                "-" +
                LineWidth.ToString();
    }

    /// <summary>The fill style of the path; null means no fill.</summary>
    public IFillType? Fill { get; }

    /// <summary>The stroke color of the path; null means no stroke (unless <see cref="LineFill"/> is set).</summary>
    public Color? LineColor { get; }

    /// <summary>Fill used to render the stroke itself.</summary>
    public IFillType? LineFill { get; }

    /// <summary>Stroke width in twips (1/20 px).</summary>
    public int LineWidth { get; }

    /// <summary>
    /// If true, edges should be added in reverse order (used by style0 fill paths).
    /// Not part of the hash.
    /// </summary>
    public bool Reverse { get; }

    /// <summary>
    /// Compute the hash code of the style to be used as a merge key.
    /// </summary>
    public string Hash() => _hash;

    /// <summary>
    /// Apply a color transform and return a new style.
    /// </summary>
    public PathStyle TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));

        var fill = (IFillType?)Fill?.TransformColors(colorTransform);
        var lineColor = LineColor?.Transform(colorTransform);
        var lineFill = (IFillType?)LineFill?.TransformColors(colorTransform);

        return new PathStyle(
            fill: fill,
            lineColor: lineColor,
            lineFill: lineFill,
            lineWidth: LineWidth,
            reverse: Reverse
        );
    }

    private static string ColorHash(Color? color)
    {
        if (color is null) return "-1";

        // Matches PHP: (r<<24)|(g<<16)|(b<<8)|(alpha ?? 255)
        int a = color.Alpha ?? 255;
        int v = (color.Red << 24) | (color.Green << 16) | (color.Blue << 8) | a;
        return v.ToString();
    }
}