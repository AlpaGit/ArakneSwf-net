namespace ArakneSwf.Parsing.Extractor.Drawer.Converters;

/// <summary>
/// Resizes an image to fit within a given box while keeping aspect ratio.
/// The larger dimension is reduced to the target size; the other scales proportionally.
/// </summary>
public sealed class FitSizeResizer : IImageResizer
{
    public int Width { get; }
    public int Height { get; }

    /// <param name="width">Target max width (positive integer).</param>
    /// <param name="height">Target max height (positive integer).</param>
    public FitSizeResizer(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Must be positive.");

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Compute the resized width and height in pixels.
    /// Mirrors the PHP logic including zero-dimension edge cases.
    /// </summary>
    public (double width, double height) Scale(double width, double height)
    {
        if (width == 0.0 && height == 0.0)
            return (Width, Height);

        if (width == 0.0)
            return (0.0, Height);

        if (height == 0.0)
            return (Width, 0.0);

        var factor = Math.Max(width / Width, height / Height);
        return (width / factor, height / factor);
    }
}