using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Record.Shape;

/// <summary>
/// FillStyle structure.
/// </summary>
public sealed class FillStyle
{
    public const int SOLID = 0x00;
    public const int LINEAR_GRADIENT = 0x10;
    public const int RADIAL_GRADIENT = 0x12;
    public const int FOCAL_GRADIENT = 0x13;
    public const int REPEATING_BITMAP = 0x40;
    public const int CLIPPED_BITMAP = 0x41;
    public const int NON_SMOOTHED_REPEATING_BITMAP = 0x42;
    public const int NON_SMOOTHED_CLIPPED_BITMAP = 0x43;

    public int Type { get; }
    public Color? Color { get; }
    public Matrix? Matrix { get; }
    public Gradient? Gradient { get; }
    public Gradient? FocalGradient { get; }
    public int? BitmapId { get; }
    public Matrix? BitmapMatrix { get; }

    public FillStyle(
        int       type,
        Color?    color         = null,
        Matrix?   matrix        = null,
        Gradient? gradient      = null,
        Gradient? focalGradient = null,
        int?      bitmapId      = null,
        Matrix?   bitmapMatrix  = null)
    {
        Type = type;
        Color = color;
        Matrix = matrix;
        Gradient = gradient;
        FocalGradient = focalGradient;
        BitmapId = bitmapId;
        BitmapMatrix = bitmapMatrix;
    }

    /// <summary>
    /// Read a single FillStyle from the stream.
    /// </summary>
    /// <param name="reader">SWF reader.</param>
    /// <param name="version">Shape tag version (1..4).</param>
    public static FillStyle Read(SwfReader reader, int version)
    {
        int type = reader.ReadUi8();

        FillStyle style;
        switch (type)
        {
            case SOLID:
                style = new FillStyle(
                    type,
                    color: version < 3 ? Color.ReadRgb(reader) : Color.ReadRgba(reader)
                );
                break;

            case LINEAR_GRADIENT:
            case RADIAL_GRADIENT:
                style = new FillStyle(
                    type,
                    matrix: Matrix.Read(reader),
                    gradient: Gradient.Read(reader, version > 2)
                );
                break;

            case FOCAL_GRADIENT:
                style = new FillStyle(
                    type,
                    matrix: Matrix.Read(reader),
                    focalGradient: Gradient.ReadFocal(reader)
                );
                break;

            case REPEATING_BITMAP:
            case CLIPPED_BITMAP:
            case NON_SMOOTHED_REPEATING_BITMAP:
            case NON_SMOOTHED_CLIPPED_BITMAP:
                style = new FillStyle(
                    type,
                    bitmapId: reader.ReadUi16(),
                    bitmapMatrix: Matrix.Read(reader)
                );
                break;

            default:
                if ((reader.Errors & Errors.InvalidData) != 0)
                    throw new ParserInvalidDataException(
                        $"Unsupported FillStyle type {type}",
                        reader.Offset
                    );
                style = new FillStyle(type); // permissif
                break;
        }

        reader.AlignByte();
        return style;
    }

    /// <summary>
    /// Read a collection of FillStyle entries.
    /// Count is UI8 (or UI16 if extended format is used and version &gt;= 2).
    /// </summary>
    public static List<FillStyle> ReadCollection(SwfReader reader, int version)
    {
        int fillStyleCount = reader.ReadUi8();
        if (version >= 2 && fillStyleCount == 0xFF)
            fillStyleCount = reader.ReadUi16();

        var list = new List<FillStyle>(fillStyleCount);
        for (int i = 0; i < fillStyleCount; i++)
        {
            list.Add(Read(reader, version));
        }

        return list;
    }
}