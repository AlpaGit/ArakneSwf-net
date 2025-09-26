namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// RemoveObject2 tag (type 28).
/// Removes a character at the specified depth.
/// </summary>
public sealed class RemoveObject2Tag
{
    public const int TYPE = 28;

    /// <summary>The depth to remove.</summary>
    public int Depth { get; }

    public RemoveObject2Tag(int depth)
    {
        Depth = depth;
    }

    /// <summary>
    /// Read a RemoveObject2Tag from the SWF reader.
    /// </summary>
    public static RemoveObject2Tag Read(SwfReader reader)
    {
        return new RemoveObject2Tag(reader.ReadUi16());
    }
}
