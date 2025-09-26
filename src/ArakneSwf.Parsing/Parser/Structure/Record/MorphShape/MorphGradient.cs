namespace ArakneSwf.Parsing.Parser.Structure.Record.MorphShape;

/// <summary>
/// MorphGradient (peu documenté dans la spec SWF).
/// Les 2 bits de poids fort = spreadMode, les 2 bits suivants = interpolationMode, 4 bits bas = nombre d'entrées.
/// </summary>
public sealed class MorphGradient
{
    public const int SPREAD_MODE_PAD = 0;
    public const int SPREAD_MODE_REFLECT = 1;
    public const int SPREAD_MODE_REPEAT = 2;

    public const int INTERPOLATION_MODE_NORMAL = 0;
    public const int INTERPOLATION_MODE_LINEAR = 1;

    /// <summary>2 bits (flags[7..6]).</summary>
    public int SpreadMode { get; }

    /// <summary>2 bits (flags[5..4]).</summary>
    public int InterpolationMode { get; }

    /// <summary>Entrées du dégradé.</summary>
    public IReadOnlyList<MorphGradientRecord> Records { get; }

    /// <summary>Point focal (seulement pour Focal Radial Gradient), sinon null.</summary>
    public float? FocalPoint { get; }

    public MorphGradient(
        int                                spreadMode,
        int                                interpolationMode,
        IReadOnlyList<MorphGradientRecord> records,
        float?                             focalPoint = null)
    {
        SpreadMode = spreadMode;
        InterpolationMode = interpolationMode;
        Records = records ?? throw new ArgumentNullException(nameof(records));
        FocalPoint = focalPoint;
    }

    /// <summary>
    /// Lecture d’un MorphGradient depuis le flux.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="focal">Indique si un point focal (Fixed8) est présent à la fin.</param>
    public static MorphGradient Read(SwfReader reader, bool focal)
    {
        byte flags = reader.ReadUi8();

        int spreadMode = (flags >> 6) & 0b11;        // 2 bits
        int interpolationMode = (flags >> 4) & 0b11; // 2 bits
        int numRecords = flags & 0b1111;             // 4 bits

        var records = ReadRecords(reader, numRecords);
        float? focalPoint = focal ? reader.ReadFixed8() : (float?)null;

        return new MorphGradient(spreadMode, interpolationMode, records, focalPoint);
    }

    private static List<MorphGradientRecord> ReadRecords(SwfReader reader, int count)
    {
        var list = new List<MorphGradientRecord>(count);

        for (int i = 0; i < count; ++i)
        {
            int startRatio = reader.ReadUi8();
            var startColor = Color.ReadRgba(reader);
            int endRatio = reader.ReadUi8();
            var endColor = Color.ReadRgba(reader);

            list.Add(new MorphGradientRecord(startRatio, startColor, endRatio, endColor));
        }

        return list;
    }
}