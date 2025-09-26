namespace ArakneSwf.Parsing.Parser.Structure.Record.Shape;

public sealed class LineStyle
{
    public int Width { get; }
    public Color? Color { get; }
    public int? StartCapStyle { get; }
    public int? JoinStyle { get; }
    public bool? HasFillFlag { get; }
    public bool? NoHScaleFlag { get; }
    public bool? NoVScaleFlag { get; }
    public bool? PixelHintingFlag { get; }
    public bool? NoClose { get; }
    public int? EndCapStyle { get; }
    public int? MiterLimitFactor { get; } // UI16 only when JoinStyle == 2 (MITER)
    public FillStyle? FillType { get; }

    public LineStyle(
        int        width,
        Color?     color            = null,
        int?       startCapStyle    = null,
        int?       joinStyle        = null,
        bool?      hasFillFlag      = null,
        bool?      noHScaleFlag     = null,
        bool?      noVScaleFlag     = null,
        bool?      pixelHintingFlag = null,
        bool?      noClose          = null,
        int?       endCapStyle      = null,
        int?       miterLimitFactor = null,
        FillStyle? fillType         = null)
    {
        Width = width;
        Color = color;
        StartCapStyle = startCapStyle;
        JoinStyle = joinStyle;
        HasFillFlag = hasFillFlag;
        NoHScaleFlag = noHScaleFlag;
        NoVScaleFlag = noVScaleFlag;
        PixelHintingFlag = pixelHintingFlag;
        NoClose = noClose;
        EndCapStyle = endCapStyle;
        MiterLimitFactor = miterLimitFactor;
        FillType = fillType;
    }

    /// <summary>
    /// Read a collection of LineStyle entries.
    /// Count is UI8 (or UI16 if the first byte is 0xFF).
    /// For version &lt; 4: entries are (UI16 width, RGB/RGBA color).
    /// For version ≥ 4: entries include cap/join flags, optional miter limit, and either RGBA color or FillStyle.
    /// </summary>
    public static List<LineStyle> ReadCollection(SwfReader reader, int version)
    {
        int lineStyleCount = reader.ReadUi8();
        if (lineStyleCount == 0xFF)
            lineStyleCount = reader.ReadUi16();

        var list = new List<LineStyle>(lineStyleCount);

        if (version < 4)
        {
            for (int i = 0; i < lineStyleCount; i++)
            {
                int width = reader.ReadUi16();
                var color = version < 3 ? Color.ReadRgb(reader) : Color.ReadRgba(reader);

                list.Add(new LineStyle(
                             width: width,
                             color: color
                         ));
            }

            return list;
        }

        // version >= 4
        for (int i = 0; i < lineStyleCount; i++)
        {
            int width = reader.ReadUi16();

            byte flags1 = reader.ReadUi8();
            int startCapStyle = (flags1 >> 6) & 0b11;        // bits 7..6
            int joinStyle = (flags1 >> 4) & 0b11;            // bits 5..4
            bool hasFillFlag = (flags1 & 0b0000_1000) != 0;  // bit 3
            bool noHScaleFlag = (flags1 & 0b0000_0100) != 0; // bit 2
            bool noVScaleFlag = (flags1 & 0b0000_0010) != 0; // bit 1
            bool pixelHinting = (flags1 & 0b0000_0001) != 0; // bit 0

            byte flags2 = reader.ReadUi8();
            // bits 7..3 reserved
            bool noClose = (flags2 & 0b0000_0100) != 0; // bit 2
            int endCapStyle = flags2 & 0b0000_0011;     // bits 1..0

            int? miterLimitFactor = (joinStyle == 2) ? reader.ReadUi16() : (int?)null; // JOIN_MITER == 2

            Color? colorVal;
            FillStyle? fillTypeVal;

            if (!hasFillFlag)
            {
                colorVal = Color.ReadRgba(reader);
                fillTypeVal = null;
            }
            else
            {
                fillTypeVal = FillStyle.Read(reader, version);
                colorVal = null;
            }

            list.Add(new LineStyle(
                         width: width,
                         color: colorVal,
                         startCapStyle: startCapStyle,
                         joinStyle: joinStyle,
                         hasFillFlag: hasFillFlag,
                         noHScaleFlag: noHScaleFlag,
                         noVScaleFlag: noVScaleFlag,
                         pixelHintingFlag: pixelHinting,
                         noClose: noClose,
                         endCapStyle: endCapStyle,
                         miterLimitFactor: miterLimitFactor,
                         fillType: fillTypeVal
                     ));
        }

        return list;
    }
}