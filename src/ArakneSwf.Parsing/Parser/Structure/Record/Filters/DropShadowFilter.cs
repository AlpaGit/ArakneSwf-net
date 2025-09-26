namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Filtre d’ombre portée (DropShadowFilter).
/// </summary>
public sealed class DropShadowFilter : Filter
{
    public const byte FILTER_ID = 0;

    public Color DropShadowColor { get; }
    public float BlurX { get; }
    public float BlurY { get; }
    public float Angle { get; }
    public float Distance { get; }
    public float Strength { get; }
    public bool InnerShadow { get; }
    public bool Knockout { get; }
    public bool CompositeSource { get; }

    /// <summary>Nombre de passes (5 bits).</summary>
    public int Passes { get; }

    public DropShadowFilter(
        Color dropShadowColor,
        float blurX,
        float blurY,
        float angle,
        float distance,
        float strength,
        bool  innerShadow,
        bool  knockout,
        bool  compositeSource,
        int   passes)
    {
        DropShadowColor = dropShadowColor;
        BlurX = blurX;
        BlurY = blurY;
        Angle = angle;
        Distance = distance;
        Strength = strength;
        InnerShadow = innerShadow;
        Knockout = knockout;
        CompositeSource = compositeSource;
        Passes = passes;
    }

    /// <summary>Lit un <see cref="DropShadowFilter"/> depuis le flux SWF.</summary>
    public static DropShadowFilter Read(SwfReader reader)
    {
        var color = Color.ReadRgba(reader);
        var blurX = reader.ReadFixed();
        var blurY = reader.ReadFixed();
        var angle = reader.ReadFixed();
        var dist = reader.ReadFixed();
        var str = reader.ReadFixed8();

        var inner = reader.ReadBool();
        var knock = reader.ReadBool();
        var compSrc = reader.ReadBool();
        var passes = (int)reader.ReadUb(5); // 5 bits

        return new DropShadowFilter(
            dropShadowColor: color,
            blurX: blurX,
            blurY: blurY,
            angle: angle,
            distance: dist,
            strength: str,
            innerShadow: inner,
            knockout: knock,
            compositeSource: compSrc,
            passes: passes
        );
    }
}