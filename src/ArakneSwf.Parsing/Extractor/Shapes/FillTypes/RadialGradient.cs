using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Shapes.FillTypes;

public sealed class RadialGradient : IFillType
{
    public RadialGradient(Matrix matrix, Gradient gradient)
    {
        Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
        Gradient = gradient ?? throw new ArgumentNullException(nameof(gradient));
    }

    public Matrix Matrix { get; }
    public Gradient Gradient { get; }

    public IFillType TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));
        return new RadialGradient(Matrix, Gradient.TransformColors(colorTransform));
    }

    public string Hash()
    {
        var json = JsonSerializer.Serialize(this, SwfJsonContext.Default.RadialGradient);
        var bytes = Encoding.UTF8.GetBytes(json);

        var hashBytes = XxHash128.Hash(bytes);
        var hex = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return "R" + hex;
    }
}