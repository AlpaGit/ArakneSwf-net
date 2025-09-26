using ArakneSwf.Parsing.Parser.Structure.Action;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DoAction tag (TYPE = 12).
/// </summary>
public sealed class DoActionTag
{
    public const int TYPE = 12;

    /// <summary>All action records contained in this tag.</summary>
    public IList<ActionRecord> Actions { get; }

    public DoActionTag(IList<ActionRecord> actions)
    {
        Actions = actions;
    }

    /// <summary>
    /// Read a DoAction tag from the reader, until the given end offset is reached.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag.</param>
    public static DoActionTag Read(SwfReader reader, int end)
    {
        var actions = ActionRecord.ReadCollection(reader, end);
        return new DoActionTag(actions);
    }
}