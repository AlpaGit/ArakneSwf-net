using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Shape extracted from a SWF file.
/// A shape contains multiple paths, has a size, and a position (offset).
/// All values are in twips (1/20th of a pixel).
/// </summary>
public sealed class Shape
{
    public Shape(
        int        width,
        int        height,
        int        xOffset,
        int        yOffset,
        List<Path> paths)
    {
        Width = width;
        Height = height;
        XOffset = xOffset;
        YOffset = yOffset;
        Paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    public int Width { get; }
    public int Height { get; }
    public int XOffset { get; }
    public int YOffset { get; }

    /// <summary>
    /// Paths to draw, ordered by drawing order.
    /// Note: line paths should be drawn after fill paths.
    /// </summary>
    public List<Path> Paths { get; }

    /// <summary>
    /// Transform the colors of all paths and return a new <see cref="Shape"/>.
    /// </summary>
    public Shape TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));

        var newPaths = new List<Path>(Paths.Count);
        foreach (var path in Paths)
        {
            newPaths.Add(path.TransformColors(colorTransform));
        }

        return new Shape(Width, Height, XOffset, YOffset, newPaths);
    }
}