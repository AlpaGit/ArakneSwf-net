namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// EnableDebugger / EnableDebugger2 tag.
/// </summary>
public sealed class EnableDebuggerTag
{
    public const int TYPE_V1 = 58;
    public const int TYPE_V2 = 64;

    /// <summary>
    /// Version of the tag: 1 (EnableDebugger) or 2 (EnableDebugger2).
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Debugger password (null-terminated string in SWF).
    /// </summary>
    public string Password { get; }

    public EnableDebuggerTag(int version, string password)
    {
        Version = version;
        Password = password;
    }

    /// <summary>
    /// Read an EnableDebugger (v1) or EnableDebugger2 (v2) tag from the reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="version">1 or 2.</param>
    public static EnableDebuggerTag Read(SwfReader reader, int version)
    {
        if (version == 2)
        {
            reader.SkipBytes(2); // Reserved, must be 0
        }

        var password = reader.ReadNullTerminatedString();

        return new EnableDebuggerTag(
            version: version,
            password: password
        );
    }
}