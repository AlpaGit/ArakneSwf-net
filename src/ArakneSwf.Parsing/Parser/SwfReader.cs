using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser;

/// <summary>
/// Low-level SWF primitives parser.
/// This class is mutable and stateful—be careful when using it.
/// </summary>
public sealed class SwfReader
{
    /// <summary>Binary data of the SWF file.</summary>
    public byte[] Data { get; }

    /// <summary>
    /// The end offset of the binary data (exclusive).
    /// No data can be read once this offset is reached.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// Flags for error reporting.
    /// If, for a given error, the corresponding bit is set, an exception will be thrown when the error occurs.
    /// If not, the error will be silently ignored, and a fallback value will be returned instead.
    /// </summary>
    public Errors Errors { get; }

    /// <summary>Current byte offset in the binary data.</summary>
    public int Offset { get; private set; } = 0;

    /// <summary>The current bit offset when reading bits (0..7).</summary>
    private int _bitOffset = 0;

    /// <summary>
    /// The current byte value used for bit operations (-1 means "no current byte").
    /// Must be reset to -1 after every change to <see cref="Offset"/>.
    /// </summary>
    private int _currentByte = -1;

    /// <param name="binary">The raw binary data of the SWF file.</param>
    /// <param name="end">The end offset of the binary data (exclusive). If null, uses the end of <paramref name="binary"/>.</param>
    /// <param name="errors">Error flags.</param>
    public SwfReader(byte[] binary, int? end = null, Errors errors = Errors.All)
    {
        if (end is { } e && e > binary.Length)
            throw new ArgumentOutOfRangeException(nameof(end), "End must be <= binary length.");

        Data = binary ?? throw new ArgumentNullException(nameof(binary));
        End = end ?? binary.Length;
        Errors = errors;
    }

    /// <summary>
    /// Uncompress the remaining data using zlib (like PHP inflate_* / gzuncompress) and return a new reader.
    /// </summary>
    /// <param name="len">
    /// Optional maximum length of the uncompressed data including data already read.
    /// If exceeded: throws <see cref="ParserExtraDataException"/> when <see cref="Errors.ExtraData"/> is set, otherwise truncates.
    /// </param>
    public SwfReader Uncompress(int? len = null)
    {
        var start = Offset;
        var end = End;

        var result = new MemoryStream();
        // Keep already-read prefix
        result.Write(Data, 0, start);

        // Stream over the compressed tail
        using var compressed = new MemoryStream(Data, start, end - start, writable: false);
        try
        {
            using var z = new ZLibStream(compressed, CompressionMode.Decompress, leaveOpen: true);
            var buf = new byte[4096];
            int read;
            while ((read = z.Read(buf, 0, buf.Length)) > 0)
            {
                result.Write(buf, 0, read);

                if (len.HasValue && result.Length > len.Value)
                {
                    break;
                }
            }
        }
        catch (InvalidDataException)
        {
            // Invalid compressed data
            if ((Errors & Errors.InvalidData) != 0)
                throw new ParserInvalidDataException("Invalid compressed data", Offset);

            // Else: ignore and return the prefix as-is
        }

        if (len.HasValue && result.Length > len.Value)
        {
            if ((Errors & Errors.ExtraData) != 0)
            {
                throw new ParserExtraDataException(
                    $"Uncompressed data exceeds the maximum length of {len.Value} bytes (actual {result.Length} bytes)",
                    Offset,
                    len.Value
                );
            }

            // Truncate
            result.SetLength(len.Value);
        }

        var bytes = result.ToArray();
        var self = new SwfReader(bytes, errors: Errors) { Offset = this.Offset };
        return self;
    }

    /// <summary>Create a new reader instance for a chunk of the binary data.</summary>
    public SwfReader Chunk(int offset, int end)
    {
        Debug.Assert(end >= offset);

        if (end > End)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadAfterEnd(end, End);

            end = End;
        }

        var self = new SwfReader(Data, end, Errors) { Offset = offset };
        return self;
    }

    public string ReadUtf(int count)
    {
        var bytes = ReadBytes(count);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>Read multiple bytes.</summary>
    public byte[] ReadBytes(int count)
    {
        Debug.Assert(_bitOffset == 0);

        if (Offset + count > End)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadTooManyBytes(Offset, End, count);

            var len = Math.Max(End - Offset, 0);
            var ret = new byte[len + Math.Min(count - len, 128)];
            if (len > 0)
                Buffer.BlockCopy(Data, Offset, ret, 0, len);
            // remaining bytes are zero (default)
            Offset = End;
            return ret;
        }

        var bytes = new byte[count];
        Buffer.BlockCopy(Data, Offset, bytes, 0, count);
        Offset += count;
        return bytes;
    }

    /// <summary>Read bytes up to the specified absolute offset (exclusive).</summary>
    public byte[] ReadBytesTo(int offset)
    {
        Debug.Assert(_bitOffset == 0);

        var cur = Offset;
        if (cur == offset) return Array.Empty<byte>();

        if (offset < cur)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw new ParserOutOfBoundException($"Cannot read bytes to an offset before the current offset: {offset} &lt; {cur}", offset);

            return Array.Empty<byte>();
        }

        if (offset > End)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadAfterEnd(cur, End);

            offset = End;
        }

        var len = offset - cur;
        var ret = new byte[len];
        if (len > 0)
            Buffer.BlockCopy(Data, cur, ret, 0, len);
        Offset = offset;
        return ret;
    }

    /// <summary>Read zlib-compressed bytes up to <paramref name="offset"/>, then uncompress them.</summary>
    public byte[] ReadZLibTo(int offset)
    {
        var compressed = ReadBytesTo(offset);
        if (compressed.Length == 0) return Array.Empty<byte>();

        try
        {
#if NET7_0_OR_GREATER
            using var input = new MemoryStream(compressed, writable: false);
            using var z = new ZLibStream(input, CompressionMode.Decompress);
#else
                using var input = new MemoryStream(compressed, writable: false);
                using var z = new DeflateStream(input, CompressionMode.Decompress);
#endif
            using var output = new MemoryStream();
            z.CopyTo(output);
            return output.ToArray();
        }
        catch (InvalidDataException)
        {
            if ((Errors & Errors.InvalidData) != 0)
                throw ParserInvalidDataException.CreateInvalidCompressedData(Offset);
            return Array.Empty<byte>();
        }
    }

    /// <summary>Skip a number of bytes without reading.</summary>
    public void SkipBytes(int count)
    {
        Debug.Assert(_bitOffset == 0);
        Offset += count;
    }

    /// <summary>Skip directly to a specific absolute offset (≥ current offset).</summary>
    public void SkipTo(int offset)
    {
        Debug.Assert(_bitOffset == 0);
        Debug.Assert(offset >= Offset);
        Offset = offset;
    }

    /// <summary>Read a single byte (like PHP <c>readChar()</c>).</summary>
    public byte ReadChar()
    {
        Debug.Assert(_bitOffset == 0);

        if (Offset >= End)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadAfterEnd(Offset, End);

            return 0x00;
        }

        return Data[Offset++];
    }

    public string ReadString()
    {
        var length = ReadU30();
        return ReadUtf(length);
    }

    /// <summary>Read a null-terminated string (bytes up to <c>\0</c>), decoded as ISO-8859-1 (raw bytes).</summary>
    public string ReadNullTerminatedString()
    {
        Debug.Assert(_bitOffset == 0);

        var pos = Offset;
        var end = Array.IndexOf<byte>(Data, 0x00, pos, End - pos);

        if (end == -1)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw new ParserInvalidDataException("String terminator not found", pos);

            var len = Math.Max(End - pos, 0);
            var s = Encoding.Latin1.GetString(Data, pos, len);
            Offset = End;
            return s;
        }

        if (end >= End)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadAfterEnd(pos, End);

            var len = Math.Max(End - pos, 0);
            var s = Encoding.Latin1.GetString(Data, pos, len);
            Offset = End;
            return s;
        }

        var ret = Encoding.Latin1.GetString(Data, pos, end - pos);
        Offset = end + 1;
        return ret;
    }

    /// <summary>Reset the bit reader state to the next byte boundary.</summary>
    public void AlignByte()
    {
        if (_bitOffset != 0)
        {
            ++Offset;
            _bitOffset = 0;
            _currentByte = -1;
        }
    }

    /// <summary>Read <paramref name="num"/> bits as an unsigned integer (0..32 bits).</summary>
    public uint ReadUb(int num)
    {
        if (num == 0) return 0u;

        uint value = 0;
        var currentByte = _currentByte;
        var bitOffset = _bitOffset;
        var streamOffset = Offset;

        while (num > 0)
        {
            if (currentByte == -1)
            {
                if (streamOffset >= End)
                {
                    if ((Errors & Errors.OutOfBounds) != 0)
                        throw ParserOutOfBoundException.CreateReadAfterEnd(streamOffset, End);

                    currentByte = 0;
                }
                else
                {
                    currentByte = Data[streamOffset];
                }
            }

            var remainingBits = 8 - bitOffset;
            var bitsToRead = remainingBits > num ? num : remainingBits;

            var mask = (1 << bitsToRead) - 1;
            var segment = (currentByte >> (remainingBits - bitsToRead)) & mask;
            value |= (uint)(segment << (num - bitsToRead));

            num -= bitsToRead;
            bitOffset += bitsToRead;

            if (bitOffset >= 8)
            {
                bitOffset = 0;
                ++streamOffset;
                currentByte = -1;
            }
        }

        // Update state
        _bitOffset = bitOffset;
        Offset = streamOffset;
        _currentByte = currentByte;

        return value;
    }

    /// <summary>Skip a number of bits (positive).</summary>
    public void SkipBits(int num)
    {
        var newOffset = _bitOffset + num;
        if (newOffset < 8)
        {
            _bitOffset = newOffset;
        }
        else
        {
            Offset += (newOffset >> 3);
            _bitOffset = newOffset & 7;
            _currentByte = -1;
        }
    }

    /// <summary>
    /// Read a signed fixed-point (16.16) number using <paramref name="num"/> bits.
    /// Follows the same sign/integer/fraction layout as the PHP implementation.
    /// </summary>
    public float ReadFb(int num)
    {
        if (num == 0) return 0f;
        Debug.Assert(num <= 32);

        var raw = ReadUb(num);
        var positive = (raw & (1u << (num - 1))) == 0;

        if (positive)
        {
            var hi = (int)((raw >> 16) & 0xffff);
            var lo = (int)(raw & 0xffff);
            return (float)(hi + (lo / 65536.0));
        }

        // Two's complement within "num" bits:
        var wide = (1UL << num) - raw;
        var hi2 = (int)(((wide >> 16) & 0xffff));
        var lo2 = (int)(wide & 0xffff);
        return (float)-(hi2 + (lo2 / 65536.0));
    }

    /// <summary>Read one bit as a boolean.</summary>
    public bool ReadBool()
    {
        var offset = _bitOffset;

        if (_currentByte == -1)
        {
            var byteOffset = Offset;
            if (byteOffset >= End)
            {
                if ((Errors & Errors.OutOfBounds) != 0)
                    throw ParserOutOfBoundException.CreateReadAfterEnd(byteOffset, End);

                _currentByte = 0;
            }
            else
            {
                _currentByte = Data[byteOffset];
            }
        }

        var mask = 1 << (7 - offset);
        var ret = (_currentByte & mask) == mask;

        ++offset;
        if (offset < 8)
        {
            _bitOffset = offset;
        }
        else
        {
            ++Offset;
            _bitOffset = 0;
            _currentByte = -1;
        }

        return ret;
    }

    /// <summary>Read a signed integer using <paramref name="num"/> bits (two's complement).</summary>
    public int ReadSb(int num)
    {
        if (num == 0) return 0;
        var val = ReadUb(num);
        var positive = (val & (1u << (num - 1))) == 0;
        return positive ? (int)val : (int)(val - (1u << num));
    }

    /// <summary>Read a fixed 8.8 number.</summary>
    public float ReadFixed8() => ReadSi16() / 256.0f;

    /// <summary>Read a fixed 16.16 number.</summary>
    public float ReadFixed() => ReadSi32() / 65536.0f;

    /// <summary>
    /// Read a SWF 16-bit float (sign:1, exponent:5 with bias 16, mantissa:10).
    /// </summary>
    public float ReadFloat16()
    {
        var raw = ReadUi8() | ((uint)ReadUi8() << 8);
        var sign = (raw >> 15) & 0x0001u;
        var exponent = (raw >> 10) & 0x001Fu;
        var mantissa = raw & 0x03FFu;

        if (exponent == 0 && mantissa == 0)
            return sign == 0 ? 0f : -0f;

        if (exponent == 0)
        {
            // Denormalized
            var val = (sign == 0 ? 1.0 : -1.0) * Math.Pow(2, -15) * mantissa / 1024.0;
            return (float)val;
        }

        if (exponent == 0x1Fu)
        {
            if (mantissa != 0) return float.NaN;
            return sign == 0 ? float.PositiveInfinity : float.NegativeInfinity;
        }

        var ret = (sign == 0 ? 1.0 : -1.0);
        if (exponent > 16) ret *= (1 << (int)(exponent - 16));
        else ret /= (1 << (int)(16 - exponent));

        ret *= (1.0 + mantissa / 1024.0);
        return (float)ret;
    }

    /// <summary>Read a 32-bit IEEE754 float (little-endian).</summary>
    public float ReadFloat()
    {
        Debug.Assert(_bitOffset == 0);

        if (Offset + 4 <= End)
        {
            var u = BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(Data, Offset, 4));
            Offset += 4;
            return BitConverter.Int32BitsToSingle((int)u);
        }

        if ((Errors & Errors.OutOfBounds) != 0)
            throw ParserOutOfBoundException.CreateReadTooManyBytes(Offset, End, 4);

        var bytes = ReadBytes(4);
        // If shorter, remaining are 0
        if (bytes.Length < 4)
        {
            Array.Resize(ref bytes, 4);
        }
        var u2 = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        return BitConverter.Int32BitsToSingle((int)u2);
    }

    /// <summary>
    /// Read a 64-bit IEEE754 double with SWF word order (high 32 then low 32, overall little-endian).
    /// Matches the PHP logic: read low(4), high(4), then interpret high+low as 'e'.
    /// </summary>
    public double ReadDouble()
    {
        var low = ReadBytes(4);
        var high = ReadBytes(4);

        var buffer = new byte[8];
        // high then low
        Buffer.BlockCopy(high, 0, buffer, 0, Math.Min(4, high.Length));
        Buffer.BlockCopy(low, 0, buffer, 4, Math.Min(4, low.Length));

        var bits = (long)BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        return BitConverter.Int64BitsToDouble(bits);
    }

    /// <summary>Read one byte as unsigned.</summary>
    public byte ReadUi8() => ReadChar();

    /// <summary>Read two bytes as signed 16-bit integer (little-endian).</summary>
    public short ReadSi16()
    {
        int lo = ReadChar();
        int hi = ReadChar();
        var v = lo | (hi << 8);
        if (v >= 32768) v -= 65536;
        return (short)v;
    }

    /// <summary>Read two bytes as unsigned 16-bit integer (little-endian).</summary>
    public ushort ReadUi16()
    {
        int lo = ReadChar();
        int hi = ReadChar();
        return (ushort)(lo | (hi << 8));
    }

    /// <summary>Peek two bytes as unsigned 16-bit integer without moving the cursor.</summary>
    public ushort PeekUi16()
    {
        var off = Offset;
        if (off + 1 >= End)
        {
            if ((Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadAfterEnd(off, End);

            return 0;
        }

        int lo = Data[off];
        int hi = Data[off + 1];
        return (ushort)(lo | (hi << 8));
    }

    /// <summary>Read signed 32-bit integer (little-endian).</summary>
    public int ReadSi32()
    {
        var v = ReadUi32();
        if (v >= 2147483648u) return (int)(v - 4294967296u);
        return (int)v;
    }

    /// <summary>Read unsigned 32-bit integer (little-endian).</summary>
    public uint ReadUi32()
    {
        Debug.Assert(_bitOffset == 0);

        if (Offset + 4 <= End)
        {
            var v = BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(Data, Offset, 4));
            Offset += 4;
            return v;
        }

        if ((Errors & Errors.OutOfBounds) != 0)
            throw ParserOutOfBoundException.CreateReadTooManyBytes(Offset, End, 4);

        var b = ReadBytes(4);
        if (b.Length < 4)
        {
            Array.Resize(ref b, 4);
        }
        return BinaryPrimitives.ReadUInt32LittleEndian(b);
    }

    /// <summary>Read signed 64-bit integer (little-endian).</summary>
    public long ReadSi64()
    {
        Debug.Assert(_bitOffset == 0);

        if (Offset + 8 <= End)
        {
            var u = BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(Data, Offset, 8));
            Offset += 8;
            return unchecked((long)u);
        }

        if ((Errors & Errors.OutOfBounds) != 0)
            throw ParserOutOfBoundException.CreateReadTooManyBytes(Offset, End, 8);

        var b = ReadBytes(8);
        if (b.Length < 8) Array.Resize(ref b, 8);
        var u2 = BinaryPrimitives.ReadUInt64LittleEndian(b);
        return unchecked((long)u2);
    }

    /// <summary>
    /// Read a variable-length encoded unsigned 32-bit integer (1..5 bytes).
    /// </summary>
    public uint ReadEncodedU32()
    {
        uint result = ReadUi8();
        if ((result & 0x00000080u) == 0) return result;

        result = (result & 0x0000007Fu) | ((uint)ReadUi8() << 7);
        if ((result & 0x00004000u) == 0) return result;

        result = (result & 0x00003FFFu) | ((uint)ReadUi8() << 14);
        if ((result & 0x00200000u) == 0) return result;

        result = (result & 0x001FFFFFu) | ((uint)ReadUi8() << 21);
        if ((result & 0x10000000u) == 0) return result;

        result = (result & 0x0FFFFFFFu) | ((uint)ReadUi8() << 28);
        return result;
    }

    public int ReadU30()
    {
        long u32 = ReadEncodedU32();
        return (int) (u32 & 0x3FFFFFFF);
    }

    public decimal ReadDecimal()
    {
        var bytes = ReadBytes(16);
        if (bytes.Length < 16)
        {
            Array.Resize(ref bytes, 16);
        }
        
        return new decimal(new[]
        {
            BitConverter.ToInt32(bytes, 0),
            BitConverter.ToInt32(bytes, 4),
            BitConverter.ToInt32(bytes, 8),
            BitConverter.ToInt32(bytes, 12)
        });
    }
}