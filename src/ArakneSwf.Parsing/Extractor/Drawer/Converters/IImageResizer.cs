namespace ArakneSwf.Parsing.Extractor.Drawer.Converters;

/// <summary>
/// Base type for computing a new image size.
/// Note: SWF uses twips (1/20 px), so fractional pixel sizes are expected.
/// </summary>
public interface IImageResizer
{
    /// <summary>
    /// Compute the resized width and height (in pixels).
    /// </summary>
    /// <param name="width">Original width in pixels.</param>
    /// <param name="height">Original height in pixels.</param>
    /// <returns>(newWidth, newHeight) in pixels.</returns>
    (double width, double height) Scale(double width, double height);
}
