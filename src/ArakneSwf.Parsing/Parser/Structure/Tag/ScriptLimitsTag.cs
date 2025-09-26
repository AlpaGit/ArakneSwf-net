namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// ScriptLimits tag (type 65).
/// Controls ActionScript recursion depth and script timeout.
/// </summary>
public sealed class ScriptLimitsTag
{
    public const int TYPE = 65;

    public int MaxRecursionDepth { get; }
    public int ScriptTimeoutSeconds { get; }

    public ScriptLimitsTag(int maxRecursionDepth, int scriptTimeoutSeconds)
    {
        MaxRecursionDepth = maxRecursionDepth;
        ScriptTimeoutSeconds = scriptTimeoutSeconds;
    }

    /// <summary>
    /// Read a ScriptLimits tag from the SWF reader.
    /// </summary>
    public static ScriptLimitsTag Read(SwfReader reader)
    {
        return new ScriptLimitsTag(
            maxRecursionDepth: reader.ReadUi16(),
            scriptTimeoutSeconds: reader.ReadUi16()
        );
    }
}
