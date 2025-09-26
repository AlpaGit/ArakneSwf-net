using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Images;

public sealed class ImageData
{
    public ImageData(ImageDataType type, byte[] data)
    {
        Type = type;
        Data = data;
    }

    public ImageDataType Type { get; }
    public byte[] Data { get; }

    /// <summary>
    /// Convert the image data to a "data:type;base64,..." string,
    /// so it can be used in href attributes or as a source for images.
    /// </summary>
    public string ToBase64Url()
    {
        return $"data:{Type.MimeType()};base64,{Convert.ToBase64String(Data)}";
    }
}
