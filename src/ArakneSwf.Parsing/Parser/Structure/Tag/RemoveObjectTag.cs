namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// RemoveObject tag (type 5).
/// Removes a specific character at the specified depth.
/// </summary>
public sealed class RemoveObjectTag
{
    public const int TYPE = 5;

    public int CharacterId { get; }
    public int Depth { get; }

    public RemoveObjectTag(int characterId, int depth)
    {
        CharacterId = characterId;
        Depth = depth;
    }

    /// <summary>
    /// Read a RemoveObjectTag from the SWF reader.
    /// </summary>
    public static RemoveObjectTag Read(SwfReader reader)
    {
        return new RemoveObjectTag(
            characterId: reader.ReadUi16(),
            depth: reader.ReadUi16()
        );
    }
}
