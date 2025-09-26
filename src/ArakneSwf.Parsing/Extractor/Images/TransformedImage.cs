using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Images.Utils;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Images;

public sealed class TransformedImage : IImageCharacter
{
    private readonly Rectangle _bounds;
    private readonly byte[] _pngData;

    private TransformedImage(int characterId, Rectangle bounds, byte[] pngData)
    {
        CharacterId = characterId;
        _bounds = bounds;
        _pngData = pngData;
    }

    public int CharacterId { get; }

    public Rectangle Bounds() => _bounds;

    public int FramesCount(bool recursive = false) => 1;

    public IImageCharacter TransformColors(ColorTransform colorTransform)
        => CreateFromPng(CharacterId, _bounds, colorTransform, _pngData);

    IDrawable IDrawable.TransformColors(ColorTransform colorTransform)
    {
        return TransformColors(colorTransform);
    }

    public string ToBase64Data()
        => $"data:image/png;base64,{Convert.ToBase64String(_pngData)}";

    public byte[] ToPng() => _pngData;

    public byte[] ToJpeg(int quality = -1)
        => Gd.FromPng(_pngData).ToJpeg(quality);

    public ImageData ToBestFormat()
        => new ImageData(ImageDataType.Png, _pngData);

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Image(this);
        return drawer;
    }


    /// <summary>
    /// Apply the color transform to PNG data and return a new instance.
    /// </summary>
    public static TransformedImage CreateFromPng(
        int            characterId,
        Rectangle      bounds,
        ColorTransform colorTransform,
        byte[]         pngData)
    {
        return CreateFromGd(characterId, bounds, colorTransform, Gd.FromPng(pngData));
    }

    /// <summary>
    /// Apply the color transform to JPEG data and return a new instance.
    /// </summary>
    public static TransformedImage CreateFromJpeg(
        int            characterId,
        Rectangle      bounds,
        ColorTransform colorTransform,
        byte[]         jpegData)
    {
        return CreateFromGd(characterId, bounds, colorTransform, Gd.FromJpeg(jpegData));
    }

    /// <summary>
    /// Apply the color transform on the parsed GD image and return a new instance.
    /// Note: the GD image is modified in place.
    /// </summary>
    public static TransformedImage CreateFromGd(
        int            characterId,
        Rectangle      bounds,
        ColorTransform colorTransform,
        Gd             image)
    {
        image.TransformColors(colorTransform);
        var pngData = image.ToPng();
        return new TransformedImage(characterId, bounds, pngData);
    }
}