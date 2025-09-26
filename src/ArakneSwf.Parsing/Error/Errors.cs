namespace ArakneSwf.Parsing.Error;

/// <summary>
/// Constants for error flags.
/// </summary>
[Flags]
public enum Errors : int
{
    /// <summary>Disable all error flags.</summary>
    None = 0,

    /// <summary>Trying to access data after the end of the input stream.</summary>
    OutOfBounds = 1 << 0,

    /// <summary>The input data is invalid or corrupted.</summary>
    InvalidData = 1 << 1,

    /// <summary>The input data has more data than expected (i.e. not all data was consumed).</summary>
    ExtraData = 1 << 2,

    /// <summary>The tag code is unknown or not supported.</summary>
    UnknownTag = 1 << 3,

    /// <summary>
    /// An error occurred while reading a tag.
    /// If this flag is not set, the tag will be skipped if an error occurs, so other errors will be ignored.
    /// Enabling all errors except this one helps detect invalid tags while remaining fail-safe.
    /// No exception is associated with this flag; it only controls whether an invalid tag is skipped or raised.
    /// </summary>
    InvalidTag = 1 << 4,

    /// <summary>A circular reference was detected during processing display list or timeline.</summary>
    CircularReference = 1 << 5,

    /// <summary>
    /// The data was successfully parsed (format is valid),
    /// but cannot be processed due to missing or incoherent data.
    /// </summary>
    UnprocessableData = 1 << 6,

    /// <summary>Enable all error flags.</summary>
    All = -1,

    /// <summary>Enable all errors, but ignore invalid tags instead of raising an error.</summary>
    IgnoreInvalidTag = All & ~InvalidTag
}
