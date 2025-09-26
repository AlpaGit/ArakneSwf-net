namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Filtre de flou (BlurFilter).
/// </summary>
public sealed class BlurFilter : Filter
{
    public const byte FILTER_ID = 1;

    public float BlurX { get; }
    public float BlurY { get; }
    /// <summary>Nombre de passes (5 bits). Les 3 bits de poids faible sont réservés.</summary>
    public int Passes { get; }

    public BlurFilter(float blurX, float blurY, int passes)
    {
        BlurX = blurX;
        BlurY = blurY;
        Passes = passes;
    }

    /// <summary>Lit un <see cref="BlurFilter"/> depuis le flux SWF.</summary>
    public static BlurFilter Read(SwfReader reader)
    {
        var blurX = reader.ReadFixed();
        var blurY = reader.ReadFixed();
        // 5 bits pour passes (bits 7..3), 3 bits réservés (bits 2..0)
        var passes = (reader.ReadUi8() >> 3) & 0b1_1111;

        return new BlurFilter(blurX, blurY, passes);
    }
}
