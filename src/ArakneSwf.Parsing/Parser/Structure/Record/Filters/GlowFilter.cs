namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Filtre de lueur (GlowFilter).
/// </summary>
public sealed class GlowFilter : Filter
{
    public const byte FILTER_ID = 2;

    public Color GlowColor { get; }
    public float BlurX { get; }
    public float BlurY { get; }
    public float Strength { get; }
    public bool InnerGlow { get; }
    public bool Knockout { get; }
    public bool CompositeSource { get; }
    /// <summary>Nombre de passes (5 bits).</summary>
    public int Passes { get; }

    public GlowFilter(
        Color glowColor,
        float blurX,
        float blurY,
        float strength,
        bool  innerGlow,
        bool  knockout,
        bool  compositeSource,
        int   passes)
    {
        GlowColor = glowColor;
        BlurX = blurX;
        BlurY = blurY;
        Strength = strength;
        InnerGlow = innerGlow;
        Knockout = knockout;
        CompositeSource = compositeSource;
        Passes = passes;
    }

    /// <summary>Lit un <see cref="GlowFilter"/> depuis le flux SWF.</summary>
    public static GlowFilter Read(SwfReader reader)
    {
        var color     = Color.ReadRgba(reader);
        var blurX   = reader.ReadFixed();
        var blurY   = reader.ReadFixed();
        var strength= reader.ReadFixed8();

        var inner    = reader.ReadBool();
        var knock    = reader.ReadBool();
        var compSrc  = reader.ReadBool();
        var passes    = (int)reader.ReadUb(5); // 5 bits

        return new GlowFilter(
            glowColor: color,
            blurX: blurX,
            blurY: blurY,
            strength: strength,
            innerGlow: inner,
            knockout: knock,
            compositeSource: compSrc,
            passes: passes
        );
    }
}
