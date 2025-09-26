namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Filtre de lueur dégradée (GradientGlowFilter).
/// </summary>
public sealed class GradientGlowFilter : Filter
{
    public const byte FILTER_ID = 4;

    public int NumColors { get; }

    /// <summary>Couleurs du dégradé (taille = <see cref="NumColors"/>).</summary>
    public IReadOnlyList<Color> GradientColors { get; }

    /// <summary>Ratios (0..255) du dégradé (taille = <see cref="NumColors"/>).</summary>
    public IReadOnlyList<int> GradientRatio { get; }

    public float BlurX { get; }
    public float BlurY { get; }
    public float Angle { get; }
    public float Distance { get; }
    public float Strength { get; }

    public bool InnerShadow { get; }
    public bool Knockout { get; }
    public bool CompositeSource { get; }
    public bool OnTop { get; }

    /// <summary>Nombre de passes (4 bits).</summary>
    public int Passes { get; }

    public GradientGlowFilter(
        int                  numColors,
        IReadOnlyList<Color> gradientColors,
        IReadOnlyList<int>   gradientRatio,
        float                blurX,
        float                blurY,
        float                angle,
        float                distance,
        float                strength,
        bool                 innerShadow,
        bool                 knockout,
        bool                 compositeSource,
        bool                 onTop,
        int                  passes)
    {
        if (gradientColors is null) throw new ArgumentNullException(nameof(gradientColors));
        if (gradientRatio is null) throw new ArgumentNullException(nameof(gradientRatio));
        if (gradientColors.Count != numColors)
            throw new ArgumentException("gradientColors.Count must equal numColors.", nameof(gradientColors));
        if (gradientRatio.Count != numColors)
            throw new ArgumentException("gradientRatio.Count must equal numColors.", nameof(gradientRatio));

        NumColors = numColors;
        GradientColors = gradientColors;
        GradientRatio = gradientRatio;
        BlurX = blurX;
        BlurY = blurY;
        Angle = angle;
        Distance = distance;
        Strength = strength;
        InnerShadow = innerShadow;
        Knockout = knockout;
        CompositeSource = compositeSource;
        OnTop = onTop;
        Passes = passes;
    }

    /// <summary>Lit un <see cref="GradientGlowFilter"/> depuis le flux SWF.</summary>
    public static GradientGlowFilter Read(SwfReader reader)
    {
        int numColors = reader.ReadUi8();

        var gradientColors = new List<Color>(numColors);
        var gradientRatio = new List<int>(numColors);

        for (var i = 0; i < numColors; ++i)
            gradientColors.Add(Color.ReadRgba(reader));

        for (var i = 0; i < numColors; ++i)
            gradientRatio.Add(reader.ReadUi8());

        var blurX = reader.ReadFixed();
        var blurY = reader.ReadFixed();
        var angle = reader.ReadFixed();
        var distance = reader.ReadFixed();
        var strength = reader.ReadFixed8();

        var innerShadow = reader.ReadBool();
        var knockout = reader.ReadBool();
        var compositeSource = reader.ReadBool();
        var onTop = reader.ReadBool();

        var passes = (int)reader.ReadUb(4); // 4 bits

        return new GradientGlowFilter(
            numColors: numColors,
            gradientColors: gradientColors,
            gradientRatio: gradientRatio,
            blurX: blurX,
            blurY: blurY,
            angle: angle,
            distance: distance,
            strength: strength,
            innerShadow: innerShadow,
            knockout: knockout,
            compositeSource: compositeSource,
            onTop: onTop,
            passes: passes
        );
    }
}