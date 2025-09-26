using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// Unknown tag.
/// Can be used to represent a tag that is not yet implemented, an error, a custom tag,
/// or an obfuscation mechanism.
/// </summary>
public sealed class UnknownTag
{
    public int Code { get; }
    public byte[] Data { get; }

    public UnknownTag(int code, byte[] data)
    {
        Code = code;
        Data = data;
    }

    /// <summary>
    /// Create an <see cref="UnknownTag"/> from the reader.
    /// </summary>
    /// <param name="reader">The SWF reader.</param>
    /// <param name="code">The tag code.</param>
    /// <param name="end">End byte offset of the tag payload.</param>
    /// <exception cref="UnknownTagException"></exception>
    public static UnknownTag Create(SwfReader reader, int code, int end)
    {
        if ((reader.Errors & Errors.UnknownTag) != 0)
        {
            throw new UnknownTagException(code, reader.Offset);
        }

        return new UnknownTag(
            code: code,
            data: reader.ReadBytesTo(end)
        );
    }
}
