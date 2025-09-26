using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Images.Utils;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Images;

/// <summary>
/// Fallback type for invalid or missing image.
/// </summary>
public sealed class EmptyImage : IImageCharacter
{
    // 1x1 transparent PNG
    public static readonly byte[] PNG_DATA =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44,
        0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x37,
        0x6E, 0xF9, 0x24, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x01, 0x63, 0x60,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0x73, 0x75, 0x01, 0x18, 0x00, 0x00, 0x00, 0x00, 0x49,
        0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    public EmptyImage(int characterId)
    {
        CharacterId = characterId;
    }

    public int CharacterId { get; }

    public int FramesCount(bool recursive = false) => 1;

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Image(this);
        return drawer;
    }

    IDrawable IDrawable.TransformColors(ColorTransform colorTransform)
    {
        return TransformColors(colorTransform);
    }

    public Rectangle Bounds()
    {
        // 20x20 twips => 1x1 pixel
        return _bounds;
    }

    public IImageCharacter TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));

        return TransformedImage.CreateFromPng(
            CharacterId,
            Bounds(),
            colorTransform,
            PNG_DATA
        );
    }

    public string ToBase64Data() => ToBestFormat().ToBase64Url();

    public byte[] ToPng() => PNG_DATA;

    public byte[] ToJpeg(int quality = -1) => Gd.FromPng(PNG_DATA).ToJpeg(quality);

    public ImageData ToBestFormat() => new ImageData(ImageDataType.Png, PNG_DATA);

    private static readonly Rectangle _bounds = new Rectangle(0, 20, 0, 20);
}