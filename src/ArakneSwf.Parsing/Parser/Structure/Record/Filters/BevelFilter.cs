namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Filtre de biseau (BevelFilter).
/// Note : la doc semble incorrecte, highlightColor vient avant shadowColor (comme en PHP).
/// </summary>
public sealed class BevelFilter : Filter
{
    public const byte FILTER_ID = 3;

    /// <summary>Couleur de surbrillance (RGBA).</summary>
    public Color HighlightColor { get; }

    /// <summary>Couleur d’ombre (RGBA).</summary>
    public Color ShadowColor { get; }

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

    public BevelFilter(
        Color highlightColor,
        Color shadowColor,
        float blurX,
        float blurY,
        float angle,
        float distance,
        float strength,
        bool  innerShadow,
        bool  knockout,
        bool  compositeSource,
        bool  onTop,
        int   passes)
    {
        HighlightColor = highlightColor;
        ShadowColor = shadowColor;
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

    /// <summary>
    /// Lit un <see cref="BevelFilter"/> depuis le flux SWF.
    /// </summary>
    public static BevelFilter Read(SwfReader reader)
    {
        // Ordre identique au PHP : highlightColor puis shadowColor
        var highlightColor = Color.ReadRgba(reader);
        var shadowColor = Color.ReadRgba(reader);

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

        return new BevelFilter(
            highlightColor,
            shadowColor,
            blurX,
            blurY,
            angle,
            distance,
            strength,
            innerShadow,
            knockout,
            compositeSource,
            onTop,
            passes
        );
    }
}