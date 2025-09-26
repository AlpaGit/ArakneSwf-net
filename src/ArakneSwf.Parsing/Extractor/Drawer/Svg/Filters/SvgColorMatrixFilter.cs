using System.Globalization;
using System.Text;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg.Filters;

public static class SvgColorMatrixFilter
{
    /// <summary>
    /// Apply a color matrix to the filter chain and return the result id.
    /// </summary>
    public static string Apply(SvgFilterBuilder builder, ColorMatrixFilter filter, string input)
    {
        // Build the values string (normalize offsets: every 5th element)
        var sb = new StringBuilder();
        for (var i = 0; i < filter.Matrix.Count; i++)
        {
            double v = filter.Matrix[i];
            if (i % 5 == 4)
                v /= 255.0;

            if (i > 0) sb.Append(' ');
            sb.Append(v.ToString(CultureInfo.InvariantCulture));
        }

        var (feColorMatrix, resultId) = builder.AddResultFilter("feColorMatrix", input);
        feColorMatrix.SetAttributeValue("type", "matrix");
        feColorMatrix.SetAttributeValue("values", sb.ToString());
        feColorMatrix.SetAttributeValue("color-interpolation-filters", "sRGB");

        return resultId;
    }
}