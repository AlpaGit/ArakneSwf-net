using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Images;

/// <summary>
/// Interface for all raster image characters defined in a SWF file.
/// </summary>
public interface IImageCharacter : IDrawable
{
    /// <summary>
    /// The character id of the image in the SWF file.
    /// </summary>
    int CharacterId { get; }

    /// <summary>
    /// Size of the image in twips.
    /// Because raster images have no offset, the bounds are always (0, 0, width, height).
    /// </summary>
    /// <remarks>
    /// Hides <see cref="IDrawable.Bounds"/> to keep the same return type but
    /// documented specifically for images.
    /// </remarks>
    new Rectangle Bounds();

    /// <summary>
    /// Transform the colors of the raster image.
    /// A new object is returned; the current instance is not modified.
    /// Note: the returned type may be a different concrete type and stores the transformed pixels.
    /// </summary>
    /// <param name="colorTransform">Color transform to apply.</param>
    /// <returns>The transformed image character.</returns>
    new IImageCharacter TransformColors(ColorTransform colorTransform);

    /// <summary>
    /// Get a data URL of the image (e.g. "data:image/png;base64,..." or "data:image/jpeg;base64,...").
    /// The best format for the current image data is chosen automatically.
    /// </summary>
    string ToBase64Data();

    /// <summary>
    /// Render the image as PNG.
    /// </summary>
    /// <returns>PNG bytes.</returns>
    byte[] ToPng();

    /// <summary>
    /// Render the image as JPEG.
    /// Note: if the image has an alpha channel, it will be lost.
    /// </summary>
    /// <param name="quality">Quality from 0 (worst) to 100 (best). If -1, an implementation-defined default is used.</param>
    /// <returns>JPEG bytes.</returns>
    byte[] ToJpeg(int quality = -1);

    /// <summary>
    /// Render the image using the best format for the current data.
    /// </summary>
    /// <returns>An <see cref="ImageData"/> describing the encoded image.</returns>
    ImageData ToBestFormat();
}
