using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Images.Utils;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Images;

/// <summary>
/// Store a raw image, extracted from a DefineBits tag.
/// Unlike <see cref="JpegImageDefinition"/>, this class only handles JPEG images and
/// requires a <see cref="JPEGTablesTag"/> to be present.
/// </summary>
public sealed class ImageBitsDefinition : IImageCharacter
{
    private Rectangle? _bounds;
    private byte[]? _fixedJpegData;

    public ImageBitsDefinition(DefineBitsTag tag, JpegTablesTag jpegTables)
    {
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        JpegTables = jpegTables ?? throw new ArgumentNullException(nameof(jpegTables));
        CharacterId = Tag.CharacterId;
    }

    public int CharacterId { get; }

    public DefineBitsTag Tag { get; }
    public JpegTablesTag JpegTables { get; }

    public Rectangle Bounds()
    {
        if (_bounds != null) return _bounds;

        // Determine size from (fixed) JPEG bytes
        var jpeg = ToJpeg();
        var gd = Gd.FromJpeg(jpeg);
        _bounds = new Rectangle(0, gd.Width * 20, 0, gd.Height * 20); // twips

        return _bounds;
    }

    public int FramesCount(bool recursive = false) => 1;

    public IImageCharacter TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));

        return TransformedImage.CreateFromJpeg(
            CharacterId,
            Bounds(),
            colorTransform,
            ToJpeg()
        );
    }

    public string ToBase64Data() =>
        "data:image/jpeg;base64," + Convert.ToBase64String(ToJpeg());

    public byte[] ToPng()
    {
        // Match PHP behavior: decode directly from tables+image, then encode PNG
        var rawJpeg = Concat(JpegtablesData(), TagImageData());
        return Gd.FromJpeg(rawJpeg).ToPng();
    }

    public byte[] ToJpeg(int quality = -1)
    {
        // Fix and cache (tables + image)
        return _fixedJpegData ??= Gd.FixJpegData(Concat(JpegtablesData(), TagImageData()));
    }

    public ImageData ToBestFormat() => new ImageData(ImageDataType.Jpeg, ToJpeg());

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Image(this);
        return drawer;
    }

    IDrawable IDrawable.TransformColors(ColorTransform colorTransform)
    {
        return TransformColors(colorTransform);
    }

    // --- helpers -------------------------------------------------------

    private byte[] JpegtablesData() => JpegTablesDataAccessor(JpegTables);
    private byte[] TagImageData()   => DefineBitsImageDataAccessor(Tag);

    private static byte[] Concat(byte[] a, byte[] b)
    {
        var res = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, res, 0, a.Length);
        Buffer.BlockCopy(b, 0, res, a.Length, b.Length);
        return res;
    }

    // Accessors kept separate to adapt easily if your generated C# model uses different property names/types.
    private static byte[] JpegTablesDataAccessor(JpegTablesTag t)
    {
        // Adjust if your generated class uses a different property (e.g., t.Bytes or t.TableData)
        return t.Data;
    }

    private static byte[] DefineBitsImageDataAccessor(DefineBitsTag t)
    {
        // Adjust if your generated class uses a different property (e.g., t.Data)
        return t.ImageData;
    }
}