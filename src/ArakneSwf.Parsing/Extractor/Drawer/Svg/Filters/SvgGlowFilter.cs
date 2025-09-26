using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg.Filters;

public static class SvgGlowFilter
{
    /// <summary>
    /// Apply the glow effect using a GlowFilter model and return the result id.
    /// </summary>
    public static string Apply(SvgFilterBuilder builder, GlowFilter filter, string input)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (filter is null) throw new ArgumentNullException(nameof(filter));
        if (input is null) throw new ArgumentNullException(nameof(input));

        if (filter.InnerGlow)
            throw new InvalidOperationException("Not implemented: inner glow filter");

        return Outer(
            builder,
            filter.GlowColor,
            filter.BlurX,
            filter.BlurY,
            filter.Passes,  
            filter.Knockout,
            input
        );
    }

    /// <summary>
    /// Outer glow is equivalent to a zero-distance, unit-strength drop shadow with blur.
    /// </summary>
    public static string Outer(
        SvgFilterBuilder builder,
        Color            color,
        double           blurX,
        double           blurY,
        int              passes,
        bool             knockout,
        string           input)
    {
        return SvgDropShadowFilter.Outer(
            builder,
            color,
            distance: 0.0,
            angle: 0.0,
            strength: 1.0,
            blurX: blurX,
            blurY: blurY,
            passes: passes,
            knockout: knockout,
            input: input
        );
    }
}