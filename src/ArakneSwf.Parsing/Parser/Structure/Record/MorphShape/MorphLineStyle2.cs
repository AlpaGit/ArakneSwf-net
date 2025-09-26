namespace ArakneSwf.Parsing.Parser.Structure.Record.MorphShape;

/// <summary>
/// Style de ligne morph v2.
/// </summary>
public sealed class MorphLineStyle2
{
    public const int CAP_ROUND = 0;
    public const int CAP_NONE = 1;
    public const int CAP_SQUARE = 2;

    public const int JOIN_ROUND = 0;
    public const int JOIN_BEVEL = 1;
    public const int JOIN_MITER = 2;

    public int StartWidth { get; }
    public int EndWidth { get; }
    public int StartCapStyle { get; }
    public int JoinStyle { get; }
    public bool NoHScale { get; }
    public bool NoVScale { get; }
    public bool PixelHinting { get; }
    public bool NoClose { get; }
    public int EndCapStyle { get; }

    /// <summary>Présent uniquement si <see cref="JoinStyle"/> == JOIN_MITER. (Fixed8)</summary>
    public float? MiterLimitFactor { get; }

    /// <summary>Présentes uniquement si pas de fill (hasFill == false).</summary>
    public Color? StartColor { get; }

    public Color? EndColor { get; }

    /// <summary>Présent uniquement si hasFill == true.</summary>
    public MorphFillStyle? FillStyle { get; }

    public MorphLineStyle2(
        int             startWidth,
        int             endWidth,
        int             startCapStyle,
        int             joinStyle,
        bool            noHScale,
        bool            noVScale,
        bool            pixelHinting,
        bool            noClose,
        int             endCapStyle,
        float?          miterLimitFactor,
        Color?          startColor,
        Color?          endColor,
        MorphFillStyle? fillStyle)
    {
        StartWidth = startWidth;
        EndWidth = endWidth;
        StartCapStyle = startCapStyle;
        JoinStyle = joinStyle;
        NoHScale = noHScale;
        NoVScale = noVScale;
        PixelHinting = pixelHinting;
        NoClose = noClose;
        EndCapStyle = endCapStyle;
        MiterLimitFactor = miterLimitFactor;
        StartColor = startColor;
        EndColor = endColor;
        FillStyle = fillStyle;
    }

    /// <summary>
    /// Lit une collection de <see cref="MorphLineStyle2"/>.
    /// Le nombre est un UI8 (ou 0xFF suivi d’un UI16 pour le format étendu).
    /// </summary>
    public static List<MorphLineStyle2> ReadCollection(SwfReader reader)
    {
        int count = reader.ReadUi8();
        if (count == 0xFF)
            count = reader.ReadUi16();

        var styles = new List<MorphLineStyle2>(count);

        for (int i = 0; i < count; i++)
        {
            int startWidth = reader.ReadUi16();
            int endWidth = reader.ReadUi16();

            // Premier octet de flags
            byte flags1 = reader.ReadUi8();
            int startCapStyle = (flags1 >> 6) & 0b11;        // bits 7..6
            int joinStyle = (flags1 >> 4) & 0b11;            // bits 5..4
            bool hasFill = (flags1 & 0b0000_1000) != 0;      // bit 3
            bool noHScale = (flags1 & 0b0000_0100) != 0;     // bit 2
            bool noVScale = (flags1 & 0b0000_0010) != 0;     // bit 1
            bool pixelHinting = (flags1 & 0b0000_0001) != 0; // bit 0

            // Second octet de flags
            byte flags2 = reader.ReadUi8();
            // bits 7..3 réservés
            bool noClose = (flags2 & 0b0000_0100) != 0; // bit 2
            int endCapStyle = flags2 & 0b0000_0011;     // bits 1..0

            float? miterLimit = (joinStyle == JOIN_MITER) ? reader.ReadFixed8() : (float?)null;

            Color? startColor = hasFill ? null : Color.ReadRgba(reader);
            Color? endColor = hasFill ? null : Color.ReadRgba(reader);

            MorphFillStyle? fillStyle = hasFill ? MorphFillStyle.Read(reader) : null;

            styles.Add(new MorphLineStyle2(
                           startWidth: startWidth,
                           endWidth: endWidth,
                           startCapStyle: startCapStyle,
                           joinStyle: joinStyle,
                           noHScale: noHScale,
                           noVScale: noVScale,
                           pixelHinting: pixelHinting,
                           noClose: noClose,
                           endCapStyle: endCapStyle,
                           miterLimitFactor: miterLimit,
                           startColor: startColor,
                           endColor: endColor,
                           fillStyle: fillStyle
                       ));
        }

        return styles;
    }
}