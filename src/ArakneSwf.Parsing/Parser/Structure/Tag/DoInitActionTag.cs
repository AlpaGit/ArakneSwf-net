using ArakneSwf.Parsing.Parser.Structure.Action;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DoInitAction tag (TYPE = 59).
/// </summary>
public sealed class DoInitActionTag
{
    public const int TYPE = 59;

    public int SpriteId { get; }
    public IReadOnlyList<ActionRecord> Actions { get; }

    public DoInitActionTag(int spriteId, IReadOnlyList<ActionRecord> actions)
    {
        SpriteId = spriteId;
        Actions  = actions;
    }

    /// <summary>
    /// Read a DoInitAction tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag's data.</param>
    public static DoInitActionTag Read(SwfReader reader, int end)
    {
        int spriteId = reader.ReadUi16();
        var actions  = ActionRecord.ReadCollection(reader, end);
        return new DoInitActionTag(spriteId, actions);
    }
}
