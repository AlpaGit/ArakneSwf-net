using System.Diagnostics;
using System.Globalization;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Représente une matrice de transformation 2D (SWF).
/// Coordonnées de translation en twips (1/20e pixel).
/// </summary>
public sealed class Matrix
{
    /// <summary>Facteur d’échelle horizontal (A).</summary>
    public float ScaleX { get; }

    /// <summary>Facteur d’échelle vertical (D).</summary>
    public float ScaleY { get; }

    /// <summary>Premier facteur de cisaillement (B).</summary>
    public float RotateSkew0 { get; }

    /// <summary>Second facteur de cisaillement (C).</summary>
    public float RotateSkew1 { get; }

    /// <summary>Translation X en twips (Tx / E).</summary>
    public int TranslateX { get; }

    /// <summary>Translation Y en twips (Ty / F).</summary>
    public int TranslateY { get; }

    public Matrix(
        float scaleX      = 1.0f,
        float scaleY      = 1.0f,
        float rotateSkew0 = 0.0f,
        float rotateSkew1 = 0.0f,
        int   translateX  = 0,
        int   translateY  = 0)
    {
        ScaleX = scaleX;
        ScaleY = scaleY;
        RotateSkew0 = rotateSkew0;
        RotateSkew1 = rotateSkew1;
        TranslateX = translateX;
        TranslateY = translateY;
    }

    /// <summary>
    /// Applique une translation (en twips) à la matrice courante et renvoie une nouvelle matrice.
    /// La translation passée est transformée par la matrice (comme en PHP).
    /// </summary>
    public Matrix Translate(int x, int y)
    {
        // PHP round() => MidpointRounding.AwayFromZero
        var tx = (int)Math.Round(ScaleX * x + RotateSkew1 * y + TranslateX, 0, MidpointRounding.AwayFromZero);
        var ty = (int)Math.Round(RotateSkew0 * x + ScaleY * y + TranslateY, 0, MidpointRounding.AwayFromZero);

        return new Matrix(ScaleX, ScaleY, RotateSkew0, RotateSkew1, tx, ty);
    }

    /// <summary>Transforme (x, y) et retourne la nouvelle abscisse.</summary>
    public int TransformX(int x, int y)
        => (int)Math.Round(ScaleX * x + RotateSkew1 * y + TranslateX, 0, MidpointRounding.AwayFromZero);

    /// <summary>Transforme (x, y) et retourne la nouvelle ordonnée.</summary>
    public int TransformY(int x, int y)
        => (int)Math.Round(RotateSkew0 * x + ScaleY * y + TranslateY, 0, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Représentation SVG: matrix(a,b,c,d,e,f). e,f sont en pixels (twips / 20).
    /// Quand <paramref name="undoTwipScale"/> est vrai, on divise aussi a,b,c,d par 20.
    /// </summary>
    public string ToSvgTransformation(bool undoTwipScale = false)
    {
        double a = ScaleX;
        double d = ScaleY;
        double b = RotateSkew0;
        double c = RotateSkew1;

        if (undoTwipScale)
        {
            a /= 20.0;
            d /= 20.0;
            b /= 20.0;
            c /= 20.0;
        }

        // e,f convertis de twips -> pixels
        var e = TranslateX / 20.0;
        var f = TranslateY / 20.0;

        // Utiliser la culture invariante pour obtenir des points comme séparateurs décimaux
        var inv = CultureInfo.InvariantCulture;
        string Fmt(double v) => Math.Round(v, 4, MidpointRounding.AwayFromZero).ToString(inv);

        return $"matrix({Fmt(a)}, {Fmt(b)}, {Fmt(c)}, {Fmt(d)}, {Fmt(e)}, {Fmt(f)})";
    }

    /// <summary>
    /// Lecture d’une matrice SWF depuis le flux binaire.
    /// </summary>
    public static Matrix Read(SwfReader reader)
    {
        var scaleX = 1.0f;
        var scaleY = 1.0f;
        var rotateSkew0 = 0.0f;
        var rotateSkew1 = 0.0f;
        var translateX = 0;
        var translateY = 0;

        // Scale
        if (reader.ReadBool())
        {
            var nScaleBits = (int)reader.ReadUb(5);
            Debug.Assert(nScaleBits < 32);
            scaleX = reader.ReadFb(nScaleBits);
            scaleY = reader.ReadFb(nScaleBits);
        }

        // Rotate/Skew
        if (reader.ReadBool())
        {
            var nRotateBits = (int)reader.ReadUb(5);
            Debug.Assert(nRotateBits < 32);
            rotateSkew0 = reader.ReadFb(nRotateBits);
            rotateSkew1 = reader.ReadFb(nRotateBits);
        }

        // Translate
        var nTranslateBits = (int)reader.ReadUb(5);
        if (nTranslateBits != 0)
        {
            Debug.Assert(nTranslateBits < 32);
            translateX = reader.ReadSb(nTranslateBits);
            translateY = reader.ReadSb(nTranslateBits);
        }

        reader.AlignByte();

        return new Matrix(scaleX, scaleY, rotateSkew0, rotateSkew1, translateX, translateY);
    }
}