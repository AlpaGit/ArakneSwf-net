using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Record.MorphShape;

/// <summary>
/// MorphFillStyle structure.
/// </summary>
public sealed class MorphFillStyle
{
    public const int SOLID = 0x00;
    public const int LINEAR_GRADIENT = 0x10;
    public const int RADIAL_GRADIENT = 0x12;
    public const int FOCAL_RADIAL_GRADIENT = 0x13;
    public const int REPEATING_BITMAP = 0x40;
    public const int CLIPPED_BITMAP = 0x41;
    public const int NON_SMOOTHED_REPEATING_BITMAP = 0x42;
    public const int NON_SMOOTHED_CLIPPED_BITMAP = 0x43;

    public int Type { get; }
    public Color? StartColor { get; }
    public Color? EndColor { get; }
    public Matrix? StartGradientMatrix { get; }
    public Matrix? EndGradientMatrix { get; }
    public MorphGradient? Gradient { get; }
    public int? BitmapId { get; }
    public Matrix? StartBitmapMatrix { get; }
    public Matrix? EndBitmapMatrix { get; }

    public MorphFillStyle(
        int            type,
        Color?         startColor          = null,
        Color?         endColor            = null,
        Matrix?        startGradientMatrix = null,
        Matrix?        endGradientMatrix   = null,
        MorphGradient? gradient            = null,
        int?           bitmapId            = null,
        Matrix?        startBitmapMatrix   = null,
        Matrix?        endBitmapMatrix     = null)
    {
        Type = type;
        StartColor = startColor;
        EndColor = endColor;
        StartGradientMatrix = startGradientMatrix;
        EndGradientMatrix = endGradientMatrix;
        Gradient = gradient;
        BitmapId = bitmapId;
        StartBitmapMatrix = startBitmapMatrix;
        EndBitmapMatrix = endBitmapMatrix;
    }

    /// <summary>
    /// Read a single <see cref="MorphFillStyle"/> from the stream.
    /// </summary>
    public static MorphFillStyle Read(SwfReader reader)
    {
        int type = reader.ReadUi8();

        switch (type)
        {
            case SOLID:
                return new MorphFillStyle(
                    type: type,
                    startColor: Color.ReadRgba(reader),
                    endColor: Color.ReadRgba(reader)
                );

            case LINEAR_GRADIENT:
            case RADIAL_GRADIENT:
                return new MorphFillStyle(
                    type: type,
                    startGradientMatrix: Matrix.Read(reader),
                    endGradientMatrix: Matrix.Read(reader),
                    gradient: MorphGradient.Read(reader, focal: false)
                );

            case FOCAL_RADIAL_GRADIENT:
                return new MorphFillStyle(
                    type: type,
                    startGradientMatrix: Matrix.Read(reader),
                    endGradientMatrix: Matrix.Read(reader),
                    gradient: MorphGradient.Read(reader, focal: true)
                );

            case REPEATING_BITMAP:
            case CLIPPED_BITMAP:
            case NON_SMOOTHED_REPEATING_BITMAP:
            case NON_SMOOTHED_CLIPPED_BITMAP:
                return new MorphFillStyle(
                    type: type,
                    bitmapId: reader.ReadUi16(),
                    startBitmapMatrix: Matrix.Read(reader),
                    endBitmapMatrix: Matrix.Read(reader)
                );

            default:
                if ((reader.Errors & Errors.InvalidData) != 0)
                    throw new ParserInvalidDataException(
                        $"Unknown MorphFillStyle type: {type}",
                        reader.Offset
                    );

                // mode permissif : on retourne un style minimal avec seulement le type
                return new MorphFillStyle(type);
        }
    }

    /// <summary>
    /// Read multiple <see cref="MorphFillStyle"/> entries.
    /// The count is a UI8, or 0xFF followed by UI16 for extended count.
    /// </summary>
    public static List<MorphFillStyle> ReadCollection(SwfReader reader)
    {
        int count = reader.ReadUi8();
        if (count == 0xFF)
            count = reader.ReadUi16();

        var styles = new List<MorphFillStyle>(count);
        for (int i = 0; i < count; ++i)
        {
            styles.Add(Read(reader));
        }

        return styles;
    }
}

