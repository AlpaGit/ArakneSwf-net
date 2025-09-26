using System.Globalization;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg.Filters;

/// <summary>
/// Blur filter helper that builds SVG filter primitives approximating Flash box blur.
/// </summary>
public static class SvgBlurFilter
{
    // Limit the box blur radius to avoid crashes or performance issues.
    // RSVG handles only 20x20 pixels for the convolution kernel.
    public const int MAX_BOX_BLUR_RADIUS = 9;

    // sqrt(3) approximates the blur box variance.
    public const double BLUR_BOX_RADIUS_TO_GAUSSIAN_BLUR_RATIO = 1.732;

    /// <summary>
    /// Apply the blur effect from a BlurFilter model.
    /// </summary>
    public static string Apply(SvgFilterBuilder builder, BlurFilter filter, string input)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (filter is null) throw new ArgumentNullException(nameof(filter));

        return Blur(builder, filter.BlurX, filter.BlurY, filter.Passes, input);
    }

    /// <summary>
    /// Create filters for a box blur (via feConvolveMatrix) or fall back to Gaussian blur if radius is large.
    /// Returns the result id of the last filter primitive.
    /// </summary>
    public static string Blur(SvgFilterBuilder builder, double blurX, double blurY, int passes, string input)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (input is null) throw new ArgumentNullException(nameof(input));

        // Fallback to Gaussian blur approximation when box kernel would exceed limits.
        if (blurX > MAX_BOX_BLUR_RADIUS || blurY > MAX_BOX_BLUR_RADIUS)
        {
            var stdDevX = blurX / BLUR_BOX_RADIUS_TO_GAUSSIAN_BLUR_RATIO;
            var stdDevY = blurY / BLUR_BOX_RADIUS_TO_GAUSSIAN_BLUR_RATIO;

            // Expand filter region a bit (≈ 3σ in each direction).
            builder.AddOffset(stdDevX * 3, stdDevY * 3);

            var (feGaussianBlur, result) = builder.AddResultFilter("feGaussianBlur", input);
            feGaussianBlur.SetAttributeValue(
                "stdDeviation",
                $"{stdDevX.ToString(CultureInfo.InvariantCulture)} {stdDevY.ToString(CultureInfo.InvariantCulture)}"
            );

            return result;
        }

        // Box kernel sizes are odd: 2*ceil(r) + 1
        var xOrder = (int)(2 * Math.Ceiling(blurX) + 1);
        var yOrder = (int)(2 * Math.Ceiling(blurY) + 1);

        var order = $"{xOrder} {yOrder}";
        var divisor = xOrder * yOrder;
        var kernelMatrix = string.Join(" ", Enumerable.Repeat("1", divisor));

        var lastResult = input;

        for (int i = 0; i < passes; i++)
        {
            var (feConvolveMatrix, result) = builder.AddResultFilter("feConvolveMatrix", lastResult);

            feConvolveMatrix.SetAttributeValue("order", order);
            feConvolveMatrix.SetAttributeValue("divisor", divisor.ToString(CultureInfo.InvariantCulture));
            feConvolveMatrix.SetAttributeValue("kernelMatrix", kernelMatrix);

            lastResult = result;
        }

        return lastResult;
    }
}