using System.Globalization;
using System.Text;
using ArakneSwf.Parsing.Extractor.Images;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Shapes.FillTypes;

public sealed class Bitmap : IFillType
{
    private readonly string _hash;

    public Bitmap(
        IImageCharacter bitmap,
        Matrix          matrix,
        bool            smoothed = true,
        bool            repeat   = false)
    {
        BitmapRef = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
        Smoothed = smoothed;
        Repeat = repeat;
        _hash = ComputeHash(BitmapRef, Matrix, Smoothed, Repeat);
    }

    public IImageCharacter BitmapRef { get; }
    public Matrix Matrix { get; }
    public bool Smoothed { get; }
    public bool Repeat { get; }

    public string Hash() => _hash;

    public IFillType TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));
        return new Bitmap(BitmapRef.TransformColors(colorTransform), Matrix, Smoothed, Repeat);
    }

    private static string ComputeHash(IImageCharacter bitmap, Matrix matrix, bool smoothed, bool repeat)
    {
        var imgHash = bitmap.CharacterId.ToString(CultureInfo.InvariantCulture);

        // When a color transform is applied, make sure that the hash is different
        if (bitmap is TransformedImage transformed)
        {
            // Assuming ToPng() returns raw PNG bytes
            var pngBytes = transformed.ToPng();
            imgHash += "-" + Crc32(pngBytes).ToString(CultureInfo.InvariantCulture);
        }

        var prefix = (repeat ? "R" : "C") + "B";
        if (!smoothed) prefix += "N";

        var matrixCrc = Crc32(Encoding.UTF8.GetBytes(matrix.ToSvgTransformation()))
            .ToString(CultureInfo.InvariantCulture);

        return $"{prefix}{imgHash}-{matrixCrc}";
    }

    // --- CRC32 (IEEE 802.3) implementation -------------------------------

    private static readonly uint[] Crc32Table = CreateCrc32Table();

    private static uint[] CreateCrc32Table()
    {
        const uint poly = 0xEDB88320u;
        var table = new uint[256];
        for (uint i = 0; i < table.Length; i++)
        {
            uint c = i;
            for (int j = 0; j < 8; j++)
            {
                c = (c & 1) != 0 ? poly ^ (c >> 1) : (c >> 1);
            }

            table[i] = c;
        }

        return table;
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc = Crc32Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }

        return ~crc;
    }
}