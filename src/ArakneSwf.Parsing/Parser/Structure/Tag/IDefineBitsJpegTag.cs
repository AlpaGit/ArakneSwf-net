using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// Interface for DefineBitsJPEG* tags carrying embedded image data.
/// </summary>
public interface IDefineBitsJpegTag
{
    /// <summary>
    /// Stored image data type (JPEG/PNG/GIF89a).
    /// </summary>
    ImageDataType Type { get; }

    /// <summary>
    /// Raw image bytes (JPEG, PNG, or GIF89a). 
    /// Use <see cref="Type"/> to determine the format.
    /// </summary>
    byte[] ImageData { get; }

    /// <summary>
    /// Uncompressed alpha bytes (one byte per pixel, opacity).
    /// Only present when <see cref="Type"/> is <see cref="ImageDataType.Jpeg"/>.
    /// Length must equal decoded image (width * height).
    /// </summary>
    byte[]? AlphaData { get; }
}
