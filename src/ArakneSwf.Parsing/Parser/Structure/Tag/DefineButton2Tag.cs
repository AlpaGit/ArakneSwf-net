using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineButton2 tag (TYPE = 34).
/// </summary>
public sealed class DefineButton2Tag
{
    public const int TYPE = 34;

    public int ButtonId { get; }
    public bool TrackAsMenu { get; }
    public int ActionOffset { get; }

    public IReadOnlyList<ButtonRecord> Characters { get; }
    public IReadOnlyList<ButtonCondAction> Actions { get; }

    public DefineButton2Tag(
        int                             buttonId,
        bool                            trackAsMenu,
        int                             actionOffset,
        IReadOnlyList<ButtonRecord>     characters,
        IReadOnlyList<ButtonCondAction> actions)
    {
        ButtonId = buttonId;
        TrackAsMenu = trackAsMenu;
        ActionOffset = actionOffset;
        Characters = characters;
        Actions = actions;
    }

    /// <summary>
    /// Read a DefineButton2 tag from the reader.
    /// </summary>
    /// <param name="reader">SWF reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag's payload.</param>
    public static DefineButton2Tag Read(SwfReader reader, int end)
    {
        int buttonId = reader.ReadUi16();

        reader.SkipBits(7); // reserved, must be 0
        bool trackAsMenu = reader.ReadBool();

        int actionOffset = reader.ReadUi16();

        // Button records (DefineButton2 => version = 2)
        var characters = ButtonRecord.ReadCollection(reader, version: 2);

        // Conditional action list
        var actions = (actionOffset != 0)
            ? ButtonCondAction.ReadCollection(reader, end)
            : [];

        return new DefineButton2Tag(buttonId, trackAsMenu, actionOffset, characters, actions);
    }
}