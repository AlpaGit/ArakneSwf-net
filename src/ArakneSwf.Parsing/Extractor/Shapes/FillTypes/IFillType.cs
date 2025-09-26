using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Shapes.FillTypes;

public interface IFillType
{
    /// <summary>
    /// Compute a stable identifier for this fill (useful for caching).
    /// </summary>
    string Hash();

    /// <summary>
    /// Return a color-transformed copy of this fill.
    /// </summary>
    /// <param name="colorTransform">The transform to apply.</param>
    /// <returns>A (possibly new) fill with transformed colors.</returns>
    IFillType TransformColors(ColorTransform colorTransform);
}