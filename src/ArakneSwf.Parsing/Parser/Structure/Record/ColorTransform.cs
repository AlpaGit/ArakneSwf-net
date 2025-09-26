using System.Diagnostics;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Stocke une transformation de couleur.
/// Les multiplicateurs sont à diviser par 256 lors de l’application.
/// </summary>
public sealed class ColorTransform
{
    public int RedMult { get; }
    public int GreenMult { get; }
    public int BlueMult { get; }
    public int AlphaMult { get; }

    public int RedAdd { get; }
    public int GreenAdd { get; }
    public int BlueAdd { get; }
    public int AlphaAdd { get; }

    public ColorTransform(
        int redMult   = 256,
        int greenMult = 256,
        int blueMult  = 256,
        int alphaMult = 256,
        int redAdd    = 0,
        int greenAdd  = 0,
        int blueAdd   = 0,
        int alphaAdd  = 0)
    {
        RedMult = redMult;
        GreenMult = greenMult;
        BlueMult = blueMult;
        AlphaMult = alphaMult;
        RedAdd = redAdd;
        GreenAdd = greenAdd;
        BlueAdd = blueAdd;
        AlphaAdd = alphaAdd;
    }

    /// <summary>
    /// Applique la transformation à une couleur et retourne le résultat (clampé à [0..255]).
    /// </summary>
    public Color Transform(Color color)
    {
        // Calculs en double pour reproduire la division PHP, puis troncature comme (int) en PHP.
        var r = color.Red * RedMult / 256.0 + RedAdd;
        var g = color.Green * GreenMult / 256.0 + GreenAdd;
        var b = color.Blue * BlueMult / 256.0 + BlueAdd;
        var a = (color.Alpha ?? (byte)255) * AlphaMult / 256.0 + AlphaAdd;

        var R = ClampToByte((int)r);
        var G = ClampToByte((int)g);
        var B = ClampToByte((int)b);
        var A = ClampToByte((int)a);

        return new Color((byte)R, (byte)G, (byte)B, (byte)A);
    }

    private static int ClampToByte(int v)
        => v < 0 ? 0 : (v > 255 ? 255 : v);

    /// <summary>
    /// Lecture d'une transformation de couleur (avec ou sans canal alpha).
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="withAlpha">Si vrai, termes alpha présents.</param>
    public static ColorTransform Read(SwfReader reader, bool withAlpha)
    {
        var hasAddTerms = reader.ReadBool();
        var hasMultTerms = reader.ReadBool();
        var nbits = (int)reader.ReadUb(4);
        Debug.Assert(nbits < 16);

        int redMult = 256, greenMult = 256, blueMult = 256, alphaMult = 256;
        int redAdd = 0, greenAdd = 0, blueAdd = 0, alphaAdd = 0;

        if (hasMultTerms)
        {
            redMult = reader.ReadSb(nbits);
            greenMult = reader.ReadSb(nbits);
            blueMult = reader.ReadSb(nbits);
            if (withAlpha)
                alphaMult = reader.ReadSb(nbits);
        }

        if (hasAddTerms)
        {
            redAdd = reader.ReadSb(nbits);
            greenAdd = reader.ReadSb(nbits);
            blueAdd = reader.ReadSb(nbits);
            if (withAlpha)
                alphaAdd = reader.ReadSb(nbits);
        }

        reader.AlignByte();

        return new ColorTransform(
            redMult,
            greenMult,
            blueMult,
            alphaMult,
            redAdd,
            greenAdd,
            blueAdd,
            alphaAdd
        );
    }
}