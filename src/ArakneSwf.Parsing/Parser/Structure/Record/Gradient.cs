using System.Text.Json.Serialization;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Dégradé (linéaire/radial/focal) avec enregistrements et modes de diffusion/interpolation.
/// </summary>
public sealed class Gradient
{
    public const int SPREAD_MODE_PAD = 0;
    public const int SPREAD_MODE_REFLECT = 1;
    public const int SPREAD_MODE_REPEAT = 2;

    public const int INTERPOLATION_MODE_NORMAL = 0;
    public const int INTERPOLATION_MODE_LINEAR = 1;

    [JsonPropertyName("spreadMode")] public int SpreadMode { get; }

    [JsonPropertyName("interpolationMode")]
    public int InterpolationMode { get; }

    [JsonPropertyName("records")] public IReadOnlyList<GradientRecord> Records { get; }

    /// <summary>Point focal (Fixed8) uniquement pour les dégradés focaux.</summary>
    [JsonPropertyName("focalPoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? FocalPoint { get; }

    public Gradient(
        int                           spreadMode,
        int                           interpolationMode,
        IReadOnlyList<GradientRecord> records,
        float?                        focalPoint = null)
    {
        SpreadMode = spreadMode;
        InterpolationMode = interpolationMode;
        Records = records ?? throw new ArgumentNullException(nameof(records));
        FocalPoint = focalPoint;
    }

    /// <summary>
    /// Retourne un nouveau <see cref="Gradient"/> avec les couleurs transformées.
    /// </summary>
    public Gradient TransformColors(ColorTransform colorTransform)
    {
        var recs = new List<GradientRecord>(Records.Count);
        foreach (var r in Records)
            recs.Add(r.TransformColors(colorTransform));

        return new Gradient(SpreadMode, InterpolationMode, recs, FocalPoint);
    }

    /// <summary>
    /// Lecture d’un dégradé simple (couleurs RGB ou RGBA selon <paramref name="withAlpha"/>).
    /// </summary>
    public static Gradient Read(SwfReader reader, bool withAlpha)
    {
        byte flags = reader.ReadUi8();
        int spreadMode = (flags >> 6) & 0b11;        // 2 bits
        int interpolationMode = (flags >> 4) & 0b11; // 2 bits
        int numRecords = flags & 0b1111;             // 4 bits

        var records = ReadRecords(reader, numRecords, withAlpha);
        return new Gradient(spreadMode, interpolationMode, records);
    }

    /// <summary>
    /// Lecture d’un dégradé focal (toujours avec alpha).
    /// </summary>
    public static Gradient ReadFocal(SwfReader reader)
    {
        byte flags = reader.ReadUi8();
        int spreadMode = (flags >> 6) & 0b11;        // 2 bits
        int interpolationMode = (flags >> 4) & 0b11; // 2 bits
        int numRecords = flags & 0b1111;             // 4 bits

        var records = ReadRecords(reader, numRecords, withAlpha: true);
        float focalPoint = reader.ReadFixed8();

        return new Gradient(spreadMode, interpolationMode, records, focalPoint);
    }

    private static List<GradientRecord> ReadRecords(SwfReader reader, int count, bool withAlpha)
    {
        var list = new List<GradientRecord>(count);
        for (int i = 0; i < count; ++i)
        {
            int ratio = reader.ReadUi8();
            var color = withAlpha ? Color.ReadRgba(reader) : Color.ReadRgb(reader);
            list.Add(new GradientRecord(ratio, color));
        }

        return list;
    }
}