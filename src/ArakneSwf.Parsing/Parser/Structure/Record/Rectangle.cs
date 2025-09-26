using System.Diagnostics;
using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Structure représentant un rectangle (deux points).
/// Coordonnées en twips (1/20e de pixel), pouvant être négatives.
/// </summary>
public sealed class Rectangle
{
    public int XMin { get; }
    public int XMax { get; }
    public int YMin { get; }
    public int YMax { get; }

    public Rectangle(int xmin, int xmax, int ymin, int ymax)
    {
        // Invariants (comme les assert PHP)
        Debug.Assert(xmin <= xmax);
        Debug.Assert(ymin <= ymax);

        if (xmin > xmax) throw new ArgumentException("xmin must be <= xmax", nameof(xmin));
        if (ymin > ymax) throw new ArgumentException("ymin must be <= ymax", nameof(ymin));

        XMin = xmin;
        XMax = xmax;
        YMin = ymin;
        YMax = ymax;
    }

    public int Width()  => XMax - XMin;
    public int Height() => YMax - YMin;

    /// <summary>
    /// Applique une matrice de transformation aux 4 coins et retourne la boîte englobante.
    /// </summary>
    public Rectangle Transform(Matrix matrix)
    {
        var xmin = int.MaxValue;
        var xmax = int.MinValue;
        var ymin = int.MaxValue;
        var ymax = int.MinValue;

        // coin (XMin, YMin)
        {
            int x = matrix.TransformX(XMin, YMin);
            int y = matrix.TransformY(XMin, YMin);
            if (x < xmin) xmin = x;
            if (x > xmax) xmax = x;
            if (y < ymin) ymin = y;
            if (y > ymax) ymax = y;
        }

        // coin (XMax, YMin)
        {
            int x = matrix.TransformX(XMax, YMin);
            int y = matrix.TransformY(XMax, YMin);
            if (x < xmin) xmin = x;
            if (x > xmax) xmax = x;
            if (y < ymin) ymin = y;
            if (y > ymax) ymax = y;
        }

        // coin (XMin, YMax)
        {
            int x = matrix.TransformX(XMin, YMax);
            int y = matrix.TransformY(XMin, YMax);
            if (x < xmin) xmin = x;
            if (x > xmax) xmax = x;
            if (y < ymin) ymin = y;
            if (y > ymax) ymax = y;
        }

        // coin (XMax, YMax)
        {
            int x = matrix.TransformX(XMax, YMax);
            int y = matrix.TransformY(XMax, YMax);
            if (x < xmin) xmin = x;
            if (x > xmax) xmax = x;
            if (y < ymin) ymin = y;
            if (y > ymax) ymax = y;
        }

        return new Rectangle(xmin, xmax, ymin, ymax);
    }

    /// <summary>
    /// Lit un rectangle depuis le flux SWF (champ RECT).
    /// </summary>
    public static Rectangle Read(SwfReader reader)
    {
        var nbits = (int)reader.ReadUb(5);
        Debug.Assert(nbits < 32);

        var xmin = reader.ReadSb(nbits);
        var xmax = reader.ReadSb(nbits);
        var ymin = reader.ReadSb(nbits);
        var ymax = reader.ReadSb(nbits);

        if (xmin > xmax)
        {
            if ((reader.Errors & Errors.InvalidData) != 0)
            {
                throw new ParserInvalidDataException(
                    $"Invalid rectangle: xmin ({xmin}) is greater than xmax ({xmax})",
                    reader.Offset
                );
            }

            // Sinon : clamp comme en PHP
            xmin = xmax;
        }

        if (ymin > ymax)
        {
            if ((reader.Errors & Errors.InvalidData) != 0)
            {
                throw new ParserInvalidDataException(
                    $"Invalid rectangle: ymin ({ymin}) is greater than ymax ({ymax})",
                    reader.Offset
                );
            }

            ymin = ymax;
        }

        var rect = new Rectangle(xmin, xmax, ymin, ymax);
        reader.AlignByte(); // aligne sur l’octet suivant (fin du champ RECT)
        return rect;
    }
}