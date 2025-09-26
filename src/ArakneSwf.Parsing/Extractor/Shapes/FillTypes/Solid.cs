using System.Globalization;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Shapes.FillTypes;

public sealed class Solid : IFillType
{
    public Solid(Color color)
    {
        Color = color ?? throw new ArgumentNullException(nameof(color));
    }

    public Color Color { get; }

    public IFillType TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));
        return new Solid(colorTransform.Transform(Color));
    }

    public string Hash()
    {
        // 'S' . ((red << 24) | (green << 16) | (blue << 8) | (alpha ?? 255))
        var a = Color.Alpha ?? 255;
        var packed = ((uint)Color.Red   << 24)
                     | ((uint)Color.Green << 16)
                     | ((uint)Color.Blue  << 8)
                     |  a;

        return "S" + packed.ToString(CultureInfo.InvariantCulture);
    }
}
