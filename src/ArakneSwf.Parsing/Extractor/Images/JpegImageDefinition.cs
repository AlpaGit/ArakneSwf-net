using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Images.Utils;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Images;

/// <summary>
/// Store a raw image, extracted from a DefineBitsJPEG tag.
/// Note: Raw image data can be JPEG/PNG/GIF89a. For JPEG, an optional alpha plane may be present.
/// </summary>
public sealed class JpegImageDefinition : IImageCharacter
{
    private Rectangle? _bounds;
    private Gd? _gd;
    private byte[]? _pngData;

    // Very small weak cache: map transform -> weak image
    private readonly List<(WeakReference<TransformedImage> Image, ColorTransform Transform)> _colorTransformCache =
        new();

    public JpegImageDefinition(DefineBitsJpeg2Tag tag)
    {
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        CharacterId = tag.CharacterId;
    }

    public JpegImageDefinition(DefineBitsJpeg3Tag tag)
    {
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        CharacterId = tag.CharacterId;
    }

    public JpegImageDefinition(DefineBitsJpeg4Tag tag)
    {
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        CharacterId = tag.CharacterId;
    }

    public int CharacterId { get; }

    public IDefineBitsJpegTag Tag { get; }

    public Rectangle Bounds()
    {
        if (_bounds != null) return _bounds;

        // Use a minimally-decoded image to get size (twips)
        var gd = Tag.Type switch
        {
            ImageDataType.Jpeg   => Gd.FromJpeg(Gd.FixJpegData(Tag.ImageData)),
            ImageDataType.Png    => Gd.FromPng(Tag.ImageData),
            ImageDataType.Gif89a => ParseGifData(), // not implemented (matching PHP)
            _                    => throw new InvalidOperationException("Unknown image type")
        };

        _bounds = new Rectangle(0, gd.Width * 20, 0, gd.Height * 20);
        return _bounds;
    }

    public int FramesCount(bool recursive = false) => 1;

    public IImageCharacter TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));

        // Try to reuse a previously produced transformed image for the same ColorTransform
        for (var i = _colorTransformCache.Count - 1; i >= 0; i--)
        {
            var (wr, tr) = _colorTransformCache[i];
            if (!wr.TryGetTarget(out var img))
            {
                _colorTransformCache.RemoveAt(i);
                continue;
            }

            if (tr.Equals(colorTransform))
                return img;
        }

        var transformed =
            (Tag.Type == ImageDataType.Jpeg && Tag.AlphaData == null)
                ? TransformedImage.CreateFromJpeg(CharacterId, Bounds(), colorTransform, Tag.ImageData)
                : TransformedImage.CreateFromGd(CharacterId, Bounds(), colorTransform, Clone(ToGD()));

        _colorTransformCache.Add((new WeakReference<TransformedImage>(transformed), colorTransform));
        return transformed;
    }

    public string ToBase64Data() => ToBestFormat().ToBase64Url();

    public byte[] ToPng()
    {
        if (Tag.Type == ImageDataType.Png)
            return Tag.ImageData;

        return _pngData ??= ToGD().ToPng();
    }

    public byte[] ToJpeg(int quality = -1)
    {
        if (Tag.Type == ImageDataType.Jpeg && Tag.AlphaData == null)
            return Gd.FixJpegData(Tag.ImageData);

        return ToGD().ToJpeg(quality);
    }

    public ImageData ToBestFormat()
    {
        if (Tag.Type == ImageDataType.Jpeg && Tag.AlphaData == null)
            return new ImageData(ImageDataType.Jpeg, Gd.FixJpegData(Tag.ImageData));

        return new ImageData(ImageDataType.Png, ToPng());
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Image(this);
        return drawer;
    }

    IDrawable IDrawable.TransformColors(ColorTransform colorTransform)
    {
        return TransformColors(colorTransform);
    }

    // --- internals -----------------------------------------------------

    private Gd ToGD()
    {
        if (_gd != null) return _gd;

        // TODO: handle deblockParam on v4 tags if needed
        _gd = Tag.Type switch
        {
            ImageDataType.Png    => Gd.FromPng(Tag.ImageData),
            ImageDataType.Gif89a => ParseGifData(),
            ImageDataType.Jpeg   => ParseJpegData(),
            _                    => throw new InvalidOperationException("Unknown image type")
        };

        return _gd;
    }

    private Gd ParseGifData()
    {
        // Match PHP behavior: not implemented yet
        throw new NotImplementedException("GIF89a decoding not implemented.");
    }

    private Gd ParseJpegData()
    {
        var gd = Gd.FromJpeg(Tag.ImageData);

        if (Tag.AlphaData is { Length: > 0 } alpha)
            ApplyAlphaChannel(gd, alpha);

        return gd;
    }

    private static Gd Clone(Gd src) => (Gd)src.Clone(); // convenience (assuming GD exposes Clone())

    private static void ApplyAlphaChannel(Gd gd, byte[] alphaData)
    {
        gd.DisableAlphaBlending();

        var width = gd.Width;
        var height = gd.Height;

        for (var y = 0; y < height; y++)
        {
            var row = y * width;
            for (var x = 0; x < width; x++)
            {
                int a = alphaData[row + x];

                if (a == 0)
                {
                    gd.SetTransparent(x, y);
                    continue;
                }

                var color = gd.Color(x, y);
                var r = (color >> 16) & 0xFF;
                var g = (color >> 8) & 0xFF;
                var b = color & 0xFF;

                gd.SetPixelAlpha(x, y, r, g, b, a);
            }
        }
    }
}