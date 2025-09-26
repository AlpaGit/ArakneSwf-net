namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// Protect tag (TYPE = 24).
/// Password is stored as an MD5 hash string when present.
/// </summary>
public sealed class ProtectTag
{
    public const int TYPE = 24;

    /// <summary>
    /// Password is an MD5 hash of the password (null when not present).
    /// </summary>
    public string? Password { get; }

    public ProtectTag(string? password)
    {
        Password = password;
    }

    /// <summary>
    /// Read a Protect tag.
    /// Password is present only when the tag body length is non-zero.
    /// </summary>
    /// <param name="reader">SWF reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset of the tag body.</param>
    public static ProtectTag Read(SwfReader reader, int end)
    {
        // Password is only present if tag length is not 0 (stored as a null-terminated string)
        return new ProtectTag(
            password: end > reader.Offset ? reader.ReadNullTerminatedString() : null
        );
    }
}
