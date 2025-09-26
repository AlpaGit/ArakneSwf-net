namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Structure pour stocker une couleur (RGBA). Canal alpha optionnel.
/// Valeurs comprises entre 0 et 255.
/// </summary>
public sealed class Color
{
    public byte Red   { get; }
    public byte Green { get; }
    public byte Blue  { get; }
    public byte? Alpha { get; }

    public Color(byte red, byte green, byte blue, byte? alpha = null)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    /// <summary>Retourne la couleur au format hexadécimal "#rrggbb" (sans alpha).</summary>
    public string Hex() => $"#{Red:x2}{Green:x2}{Blue:x2}";

    /// <summary>Opacité dans [0.0;1.0]. Si alpha absent, retourne 1.0.</summary>
    public float Opacity() => Alpha.HasValue ? Alpha.Value / 255f : 1f;

    /// <summary>Vrai si alpha est présent et différent de 255.</summary>
    public bool HasTransparency() => Alpha.HasValue && Alpha.Value < 255;

    public override string ToString() => Hex();

    /// <summary>
    /// Applique une transformation de couleur et retourne la nouvelle couleur.
    /// </summary>
    public Color Transform(ColorTransform colorTransform) => colorTransform.Transform(this);

    /// <summary>Lit une couleur RGB (3 octets) depuis le flux.</summary>
    public static Color ReadRgb(SwfReader reader)
        => new Color(reader.ReadUi8(), reader.ReadUi8(), reader.ReadUi8());

    /// <summary>Lit une couleur RGBA (4 octets) depuis le flux.</summary>
    public static Color ReadRgba(SwfReader reader)
        => new Color(reader.ReadUi8(), reader.ReadUi8(), reader.ReadUi8(), reader.ReadUi8());
}
