namespace ArakneSwf.Parsing.Parser.Structure.Record.MorphShape;

/// <summary>
/// Enregistrement d’un dégradé morph : (startRatio, startColor) → (endRatio, endColor).
/// </summary>
public sealed class MorphGradientRecord
{
    /// <summary>Ratio de départ (UI8, 0..255).</summary>
    public int StartRatio { get; }

    /// <summary>Couleur de départ (RGBA).</summary>
    public Color StartColor { get; }

    /// <summary>Ratio d’arrivée (UI8, 0..255).</summary>
    public int EndRatio { get; }

    /// <summary>Couleur d’arrivée (RGBA).</summary>
    public Color EndColor { get; }

    public MorphGradientRecord(int startRatio, Color startColor, int endRatio, Color endColor)
    {
        StartRatio = startRatio;
        StartColor = startColor ?? throw new ArgumentNullException(nameof(startColor));
        EndRatio   = endRatio;
        EndColor   = endColor   ?? throw new ArgumentNullException(nameof(endColor));
    }
}
