using ArakneSwf.Parsing.Parser.Structure.Action;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineButton tag (TYPE = 7).
/// </summary>
public sealed class DefineButtonTag
{
    public const int TYPE = 7;

    public int ButtonId { get; }
    public IReadOnlyList<ButtonRecord> Characters { get; }
    public IReadOnlyList<ActionRecord> Actions { get; }

    public DefineButtonTag(
        int                         buttonId,
        IReadOnlyList<ButtonRecord> characters,
        IReadOnlyList<ActionRecord> actions)
    {
        ButtonId = buttonId;
        Characters = characters;
        Actions = actions;
    }

    /// <summary>
    /// Read a DefineButtonTag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) for this tag.</param>
    public static DefineButtonTag Read(SwfReader reader, int end)
    {
        int buttonId = reader.ReadUi16();
        var characters = ButtonRecord.ReadCollection(reader, version: 1);
        var actions = ActionRecord.ReadCollection(reader, end);
        return new DefineButtonTag(buttonId, characters, actions);
    }
}