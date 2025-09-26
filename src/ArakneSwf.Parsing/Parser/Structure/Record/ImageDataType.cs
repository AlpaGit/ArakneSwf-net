using System.Text;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>Type of the image data in JPEG-based tags (can actually be PNG/GIF too).</summary>
public enum ImageDataType
{
    Jpeg,
    Png,
    Gif89a
}

public static class ImageDataTypeExtensions
{
    private static readonly byte[] PNG_HEADER = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] GIF89A_HEADER = Encoding.ASCII.GetBytes("GIF89a");

    /// <summary>Get the MIME type.</summary>
    public static string MimeType(this ImageDataType t) => t switch
    {
        ImageDataType.Jpeg   => "image/jpeg",
        ImageDataType.Png    => "image/png",
        ImageDataType.Gif89a => "image/gif",
        _                    => "application/octet-stream"
    };

    /// <summary>Get the file extension (without dot).</summary>
    public static string Extension(this ImageDataType t) => t switch
    {
        ImageDataType.Jpeg   => "jpg",
        ImageDataType.Png    => "png",
        ImageDataType.Gif89a => "gif",
        _                    => "bin"
    };

    /// <summary>
    /// Resolve the image data type from the raw bytes header.
    /// Defaults to JPEG if no PNG/GIF89a signature is found.
    /// </summary>
    public static ImageDataType Resolve(ReadOnlySpan<byte> imageData)
    {
        if (StartsWith(imageData, PNG_HEADER))
            return ImageDataType.Png;

        if (StartsWith(imageData, GIF89A_HEADER))
            return ImageDataType.Gif89a;

        return ImageDataType.Jpeg;
    }
    

    private static bool StartsWith(ReadOnlySpan<byte> data, ReadOnlySpan<byte> prefix)
    {
        if (data.Length < prefix.Length) return false;
        for (int i = 0; i < prefix.Length; i++)
            if (data[i] != prefix[i])
                return false;
        return true;
    }
}