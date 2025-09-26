using System.Globalization;
using System.Xml.Linq;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg.Filters;

public static class SvgDropShadowFilter
{
    /// <summary>
    /// Apply the drop shadow effect to the builder using a DropShadowFilter model.
    /// Returns the result id to be used as the "in" of subsequent primitives.
    /// </summary>
    public static string Apply(SvgFilterBuilder builder, DropShadowFilter filter, string input)
    {
        if (filter is null) throw new ArgumentNullException(nameof(filter));
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (input is null) throw new ArgumentNullException(nameof(input));

        if (filter.InnerShadow)
            throw new InvalidOperationException("Inner shadow is not supported");

        return Outer(
            builder,
            filter.DropShadowColor,
            filter.Distance,
            filter.Angle,
            filter.Strength,
            filter.BlurX,
            filter.BlurY,
            filter.Passes,
            filter.Knockout,
            input
        );
    }

    /// <summary>
    /// Build an outer drop shadow chain and return the final result id.
    /// </summary>
    public static string Outer(
        SvgFilterBuilder builder,
        Color            color,
        double           distance,
        double           angle,
        double           strength,
        double           blurX,
        double           blurY,
        int              passes,
        bool             knockout,
        string           input)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (input is null) throw new ArgumentNullException(nameof(input));

        // Offset vector (angle assumed in radians, as in PHP's cos/sin).
        var dx = distance * Math.Cos(angle);
        var dy = distance * Math.Sin(angle);

        var lastResult = input;

        // Apply offset if any.
        if (dx != 0.0 || dy != 0.0)
        {
            var (feOffset, resultId) = builder.AddResultFilter("feOffset", input);
            feOffset.SetAttributeValue("dx", dx.ToString(CultureInfo.InvariantCulture));
            feOffset.SetAttributeValue("dy", dy.ToString(CultureInfo.InvariantCulture));
            lastResult = resultId;
        }

        // Colorize to the shadow color using a color matrix.
        {
            var (feColorMatrix, resultId) = builder.AddResultFilter("feColorMatrix", lastResult);
            feColorMatrix.SetAttributeValue("type", "matrix");

            // Matrix that maps the source to the specified RGB with alpha scaled by strength.
            // Row-major 4x5:
            // [0 0 0 0 r] [0 0 0 0 g] [0 0 0 0 b] [0 0 0 a 0]
            // with r,g,b in [0,1], a = color.opacity * strength
            var values =
                "0 0 0 0 " +
                (color.Red / 255.0).ToString(CultureInfo.InvariantCulture) +
                " " +
                "0 0 0 0 " +
                (color.Green / 255.0).ToString(CultureInfo.InvariantCulture) +
                " " +
                "0 0 0 0 " +
                (color.Blue / 255.0).ToString(CultureInfo.InvariantCulture) +
                " " +
                "0 0 0 " +
                (color.Opacity() * strength).ToString(CultureInfo.InvariantCulture) +
                " 0";

            feColorMatrix.SetAttributeValue("values", values);
            lastResult = resultId;
        }

        // Blur the shadow.
        lastResult = SvgBlurFilter.Blur(builder, blurX, blurY, passes, lastResult);

        // If knockout, return only the shadow result.
        if (knockout)
            return lastResult;

        // Merge shadow with original input.
        var (feMerge, mergeResult) = builder.AddResultFilter("feMerge");
        var feMergeNode1 = new XElement("feMergeNode");
        feMergeNode1.SetAttributeValue("in", lastResult);
        feMerge.Add(feMergeNode1);

        var feMergeNode2 = new XElement("feMergeNode");
        feMergeNode2.SetAttributeValue("in", input);
        feMerge.Add(feMergeNode2);

        return mergeResult;
    }
}