using ArakneSwf.Parsing.Parser.Structure.Record;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ArakneSwf.Parsing.Extractor.Images.Utils;


/// <summary>
/// ImageSharp-based equivalent of the PHP GD wrapper.
/// </summary>
public sealed class Gd : IDisposable, ICloneable
{
    private Image<Rgba32> _image;

    private Gd(Image<Rgba32> image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
        Width = _image.Width;
        Height = _image.Height;
    }

    public int Width { get; }
    public int Height { get; }

    /// <summary>No-op (kept for API parity). ImageSharp keeps full alpha.</summary>
    public void DisableAlphaBlending()
    {
        /* no blending state in ImageSharp */
    }

    /// <summary>
    /// Set a pixel with alpha. Inputs r,g,b are expected premultiplied by alpha (like the PHP version).
    /// We un-premultiply before writing.
    /// </summary>
    public void SetPixelAlpha(int x, int y, int red, int green, int blue, int alpha)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;

        if (alpha <= 0)
        {
            _image[x, y] = new Rgba32(0, 0, 0, 0);
            return;
        }

        var factor = 255.0 / alpha;
        var r = ClampToByte((int)(red * factor));
        var g = ClampToByte((int)(green * factor));
        var b = ClampToByte((int)(blue * factor));
        var a = ClampToByte(alpha);

        _image[x, y] = new Rgba32(r, g, b, a);
    }

    /// <summary>Set an opaque pixel.</summary>
    public void SetPixel(int x, int y, int red, int green, int blue)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;
        _image[x, y] = new Rgba32(ClampToByte(red), ClampToByte(green), ClampToByte(blue), 255);
    }

    /// <summary>Set fully transparent pixel.</summary>
    public void SetTransparent(int x, int y)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;
        _image[x, y] = new Rgba32(0, 0, 0, 0);
    }

    /// <summary>Return ARGB (0xAARRGGBB) at (x,y).</summary>
    public int Color(int x, int y)
    {
        var p = _image[x, y];
        return (p.A << 24) | (p.R << 16) | (p.G << 8) | p.B;
    }

    /// <summary>
    /// Apply SWF ColorTransform (8.8 fixed multipliers + adds) per pixel.
    /// Clamps to 0..255 (alpha too, unlike GD’s 0..127 quirk).
    /// </summary>
    public void TransformColors(ColorTransform matrix)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));

        _image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    ref var p = ref row[x];

                    var r = ((p.R * matrix.RedMult) >> 8) + matrix.RedAdd;
                    var g = ((p.G * matrix.GreenMult) >> 8) + matrix.GreenAdd;
                    var b = ((p.B * matrix.BlueMult) >> 8) + matrix.BlueAdd;
                    var a = ((p.A * matrix.AlphaMult) >> 8) + matrix.AlphaAdd;

                    p.R = ClampToByte(r);
                    p.G = ClampToByte(g);
                    p.B = ClampToByte(b);
                    p.A = ClampToByte(a);
                }
            }
        });
    }

    /// <summary>Encode PNG. Compression argument is ignored (ImageSharp picks defaults).</summary>
    public byte[] ToPng(int compression = -1)
    {
        using var ms = new MemoryStream();
        var enc = new PngEncoder(); // could set CompressionLevel if desired
        _image.Save(ms, enc);
        return ms.ToArray();
    }

    /// <summary>Encode JPEG. Quality 0..100; -1 uses default.</summary>
    public byte[] ToJpeg(int quality = -1)
    {
        using var ms = new MemoryStream();
        var enc = new JpegEncoder()
        {
            Quality = quality is >= 0 and <= 100 ? quality : 75
        };
        _image.Save(ms, enc);
        return ms.ToArray();
    }

    public void Dispose()
    {
        _image?.Dispose();
        _image = null!;
    }

    public object Clone() => new Gd(_image.CloneAs<Rgba32>());

    // --- Factories ------------------------------------------------------

    /// <summary>Create from JPEG bytes; tries to sanitize broken SWF JPEG data.</summary>
    public static Gd FromJpeg(byte[] jpegData)
    {
        if (jpegData is null) throw new ArgumentNullException(nameof(jpegData));
        var fixedBytes = FixJpegData(jpegData);
        var img = Image.Load<Rgba32>(fixedBytes);
        return new Gd(img);
    }

    /// <summary>Create from PNG bytes.</summary>
    public static Gd FromPng(byte[] pngData)
    {
        if (pngData is null) throw new ArgumentNullException(nameof(pngData));
        var img = Image.Load<Rgba32>(pngData);
        return new Gd(img);
    }

    /// <summary>Create an RGBA image (truecolor). ImageSharp has no palette mode like GD; we return RGBA.</summary>
    public static Gd CreateWithColorPallet(int width, int height) => Create(width, height);

    /// <summary>Create an RGBA image (truecolor).</summary>
    public static Gd Create(int width, int height)
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();
        return new Gd(new Image<Rgba32>(width, height));
    }

    /// <summary>
    /// Clean SWF-corrupted JPEG data (duplicate SOI/EOI, invalid headers), similar to the PHP version.
    /// </summary>
    public static byte[] FixJpegData(byte[] data)
    {
        if (data is null || data.Length == 0) return new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };

        var len = data.Length;
        using var ms = new MemoryStream(len + 8);

        // Write valid SOI
        ms.WriteByte(0xFF);
        ms.WriteByte(0xD8);

        var pos = 0;
        while (true)
        {
            var next = IndexOfFf(data, pos);
            if (next < 0 || next >= len - 1) break;

            var marker = data[next + 1];

            // Skip SOI(0xD8) and EOI(0xD9)
            if (marker == 0xD8 || marker == 0xD9)
            {
                if (next > pos) ms.Write(data, pos, next - pos);
                pos = next + 2;
                continue;
            }

            var hasLength = marker != 0x00 && (marker < 0xD0 || marker > 0xD7) && (next + 3 < len);
            if (hasLength)
            {
                var length = (data[next + 2] << 8) + data[next + 3];
                var chunkEnd = next + 2 + length;
                if (chunkEnd > len) break;

                if (next > pos) ms.Write(data, pos, next - pos);
                ms.Write(data, next, 2 + length);
                pos = chunkEnd;
            }
            else
            {
                if (next > pos) ms.Write(data, pos, next - pos);
                ms.WriteByte(0xFF);
                ms.WriteByte(marker);
                pos = next + 2;
            }
        }

        // EOI
        ms.WriteByte(0xFF);
        ms.WriteByte(0xD9);

        return ms.ToArray();
    }

    // --- Helpers --------------------------------------------------------

    private static int IndexOfFf(byte[] data, int start)
    {
        for (var i = start; i < data.Length - 1; i++)
            if (data[i] == 0xFF)
                return i;
        return -1;
    }

    private static byte ClampToByte(int v) => (byte)(v < 0 ? 0 : (v > 255 ? 255 : v));
}