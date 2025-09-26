namespace ArakneSwf.Parsing.Error;

/// <summary>
/// Thrown when a tag code is not recognized by the parser.
/// </summary>
public sealed class UnknownTagException : Exception
{
    /// <summary>Tag code that was not recognized (non-negative).</summary>
    public int TagCode { get; }

    /// <summary>Byte offset in the stream where the tag was encountered (non-negative).</summary>
    public int Offset { get; }

    public UnknownTagException(int tagCode, int offset)
        : base($"Unknown tag with code {tagCode} at offset {offset}")
    {
        TagCode = tagCode;
        Offset = offset;

        // Align with PHP version that uses Errors::UNKNOWN_TAG as code.
        // Assuming an Errors class with a matching constant exists.
        // If not, remove this line or replace with your own code.
        HResult = (int)Errors.UnknownTag;
    }
}
