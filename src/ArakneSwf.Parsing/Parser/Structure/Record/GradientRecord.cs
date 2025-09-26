namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// A single gradient stop: (ratio, color).
/// Ratio is the distance within the gradient box (0 = start, 255 = end).
/// </summary>
public sealed class GradientRecord
{
    /// <summary>
    /// Distance from start of the gradient box (0..255).
    /// </summary>
    public int Ratio { get; }

    public Color Color { get; }

    public GradientRecord(int ratio, Color color)
    {
        Ratio = ratio;
        Color = color;
    }

    /// <summary>
    /// Apply a color transform and return a new <see cref="GradientRecord"/>.
    /// </summary>
    public GradientRecord TransformColors(ColorTransform colorTransform)
    {
        return new GradientRecord(Ratio, colorTransform.Transform(Color));
    }
}