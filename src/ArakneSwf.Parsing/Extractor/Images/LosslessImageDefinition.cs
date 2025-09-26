using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Images.Utils;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Images;

/// <summary>
/// Store a raw image, extracted from a DefineBitsLossless tag.
/// Best export format is PNG.
/// </summary>
public sealed class LosslessImageDefinition : IImageCharacter
{
    private Rectangle? _bounds;
    private Gd? _gd;
    private byte[]? _pngData;

    public LosslessImageDefinition(DefineBitsLosslessTag tag)
    {
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        CharacterId = Tag.CharacterId;
    }

    public int CharacterId { get; }
    public DefineBitsLosslessTag Tag { get; }

    public Rectangle Bounds() =>
        _bounds ??= new Rectangle(0, Tag.BitmapWidth * 20, 0, Tag.BitmapHeight * 20);

    public int FramesCount(bool recursive = false) => 1;

    public IImageCharacter TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));
        return TransformedImage.CreateFromGd(CharacterId, Bounds(), colorTransform, (Gd)ToGd().Clone());
    }

    public string ToBase64Data() => ToBestFormat().ToBase64Url();

    public byte[] ToPng() => _pngData ??= ToGd().ToPng();

    public byte[] ToJpeg(int quality = -1) => ToGd().ToJpeg(quality);

    public ImageData ToBestFormat() => new ImageData(ImageDataType.Png, ToPng());

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Image(this);
        return drawer;
    }

    IDrawable IDrawable.TransformColors(ColorTransform colorTransform)
    {
        return TransformColors(colorTransform);
    }

    // ------------------------- internals -------------------------

    private Gd ToGd()
    {
        if (_gd != null) return _gd;

        int width = Tag.BitmapWidth;
        int height = Tag.BitmapHeight;

        if (width < 1 || height < 1)
            throw new InvalidOperationException("Empty image is not supported.");

        // You can choose to create a palette image for 8-bit cases.
        // If your GD wrapper does not have palette support, using Create() is fine for all.
        var gd = Tag.BitmapType.IsTrueColor()
            ? Gd.Create(width, height)
            : Gd.CreateWithColorPallet(width, height);

        switch (Tag.BitmapType)
        {
            case ImageBitmapType.Opaque8Bit:
                Decode8Bit(gd, width, height);
                break;

            case ImageBitmapType.Opaque24Bit:
                Decode24Bit(gd, width, height);
                break;

            case ImageBitmapType.Opaque15Bit:
                throw new NotImplementedException("Opaque15Bit is not implemented yet.");

            case ImageBitmapType.Transparent8Bit:
                Decode8BitWithAlpha(gd, width, height);
                break;

            case ImageBitmapType.Transparent32Bit:
                Decode32BitWithAlpha(gd, width, height);
                break;

            default:
                throw new InvalidOperationException("Unknown lossless bitmap type.");
        }

        _gd = gd;
        return gd;
    }

    private void Decode8Bit(Gd gd, int width, int height)
    {
        var table = Tag.ColorTable ?? throw new InvalidOperationException("Color table is missing for 8-bit image.");
        int colorCount = table.Length / 3;
        var colors = new (byte R, byte G, byte B)[colorCount];
        for (int i = 0, c = 0; i + 2 < table.Length; i += 3, c++)
        {
            colors[c] = (table[i], table[i + 1], table[i + 2]);
        }

        SetColorMapPixelsRgb(gd, colors, width, height, Tag.PixelData);
    }

    private void Decode8BitWithAlpha(Gd gd, int width, int height)
    {
        var table = Tag.ColorTable ?? throw new InvalidOperationException("Color table is missing for 8-bit image.");
        int colorCount = table.Length / 4;
        var colors = new (byte R, byte G, byte B, byte A)[colorCount];
        for (int i = 0, c = 0; i + 3 < table.Length; i += 4, c++)
        {
            colors[c] = (table[i], table[i + 1], table[i + 2], table[i + 3]);
        }

        SetColorMapPixelsRgba(gd, colors, width, height, Tag.PixelData);
    }

    private static void SetColorMapPixelsRgb(Gd     gd, (byte R, byte G, byte B)[] colors, int width, int height,
                                             byte[] pixelData)
    {
        // Each scanline is padded to 32-bit. Padding bytes follow the indices.
        int paddingSize = (4 - (width % 4)) & 3;

        for (int y = 0; y < height; y++)
        {
            int rowStart = y * (width + paddingSize);
            for (int x = 0; x < width; x++)
            {
                byte idx = pixelData[rowStart + x];
                var (r, g, b) = colors[idx];
                gd.SetPixel(x, y, r, g, b);
            }
        }
    }

    private static void SetColorMapPixelsRgba(Gd gd, (byte R, byte G, byte B, byte A)[] colors, int width, int height,
                                              byte[] pixelData)
    {
        int paddingSize = (4 - (width % 4)) & 3;

        for (int y = 0; y < height; y++)
        {
            int rowStart = y * (width + paddingSize);
            for (int x = 0; x < width; x++)
            {
                byte idx = pixelData[rowStart + x];
                var (r, g, b, a) = colors[idx];

                if (a == 0)
                    gd.SetTransparent(x, y);
                else
                    gd.SetPixelAlpha(x, y, r, g, b, a);
            }
        }
    }

    private void Decode32BitWithAlpha(Gd gd, int width, int height)
    {
        gd.DisableAlphaBlending();

        var data = Tag.PixelData;
        int len = width * height * 4;

        for (int i = 0; i < len; i += 4)
        {
            int pixel = i >> 2;
            int x = pixel % width;
            int y = pixel / width;

            byte a = data[i + 0];
            byte r = data[i + 1];
            byte g = data[i + 2];
            byte b = data[i + 3];

            if (a == 0)
                gd.SetTransparent(x, y);
            else
                gd.SetPixelAlpha(x, y, r, g, b, a);
        }
    }

    private void Decode24Bit(Gd gd, int width, int height)
    {
        // Pixels are 32-bit aligned: [pad][R][G][B]
        var data = Tag.PixelData;
        int len = width * height * 4;

        for (int i = 0; i < len; i += 4)
        {
            byte r = data[i + 1];
            byte g = data[i + 2];
            byte b = data[i + 3];

            int pixel = i >> 2;
            int x = pixel % width;
            int y = pixel / width;

            gd.SetPixel(x, y, r, g, b);
        }
    }
}