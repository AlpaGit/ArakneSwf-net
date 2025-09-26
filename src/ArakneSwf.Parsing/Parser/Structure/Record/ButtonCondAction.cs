using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Parser.Structure.Action;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// ButtonCondAction: conditions + bloc d’actions pour un bouton.
/// </summary>
public sealed class ButtonCondAction
{
    public const int KEY_LEFT_ARROW = 1;
    public const int KEY_RIGHT_ARROW = 2;
    public const int KEY_HOME = 3;
    public const int KEY_END = 4;
    public const int KEY_INSERT = 5;
    public const int KEY_DELETE = 6;
    public const int KEY_BACKSPACE = 8;
    public const int KEY_ENTER = 13;
    public const int KEY_UP_ARROW = 14;
    public const int KEY_DOWN_ARROW = 15;
    public const int KEY_PAGE_UP = 16;
    public const int KEY_PAGE_DOWN = 17;
    public const int KEY_TAB = 18;
    public const int KEY_ESCAPE = 19;

    public int Size { get; }
    public bool IdleToOverDown { get; }
    public bool OutDownToIdle { get; }
    public bool OutDownToOverDown { get; }
    public bool OverDownToOutDown { get; }
    public bool OverDownToOverUp { get; }
    public bool OverUpToOverDown { get; }
    public bool OverUpToIdle { get; }
    public bool IdleToOverUp { get; }

    /// <summary>
    /// Code touche déclencheur. SWF4 et antérieur: 0.
    /// SWF ≥ 5: l’une des constantes KEY_* ci-dessus, ou code ASCII 32..126.
    /// </summary>
    public int KeyPress { get; }

    public bool OverDownToIdle { get; }

    public IReadOnlyList<ActionRecord> Actions { get; }

    public ButtonCondAction(
        int                         size,
        bool                        idleToOverDown,
        bool                        outDownToIdle,
        bool                        outDownToOverDown,
        bool                        overDownToOutDown,
        bool                        overDownToOverUp,
        bool                        overUpToOverDown,
        bool                        overUpToIdle,
        bool                        idleToOverUp,
        int                         keyPress,
        bool                        overDownToIdle,
        IReadOnlyList<ActionRecord> actions)
    {
        Size = size;
        IdleToOverDown = idleToOverDown;
        OutDownToIdle = outDownToIdle;
        OutDownToOverDown = outDownToOverDown;
        OverDownToOutDown = overDownToOutDown;
        OverDownToOverUp = overDownToOverUp;
        OverUpToOverDown = overUpToOverDown;
        OverUpToIdle = overUpToIdle;
        IdleToOverUp = idleToOverUp;
        KeyPress = keyPress;
        OverDownToIdle = overDownToIdle;
        Actions = actions;
    }

    /// <summary>
    /// Parse une collection de ButtonCondAction jusqu’à l’offset <paramref name="end"/>.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="end">Offset fin (exclusif) du bloc en octets.</param>
    public static List<ButtonCondAction> ReadCollection(SwfReader reader, int end)
    {
        var list = new List<ButtonCondAction>();
        int size;

        do
        {
            var start = reader.Offset;
            size = reader.ReadUi16();

            // taille minimale : 4 (2 pour size, 2 pour flags). 0 signifie "dernier enregistrement jusqu'à la fin".
            if (size != 0 && size < 4)
            {
                if ((reader.Errors & Errors.InvalidData) != 0)
                    throw new ParserInvalidDataException($"Invalid ButtonCondAction size: {size}", start);

                // Mode permissif : ignorer le record et avancer à la fin.
                reader.SkipTo(end);
                break;
            }

            // Premier octet de flags (états de transition)
            var f1 = reader.ReadUi8();
            var idleToOverDown = (f1 & 0b1000_0000) != 0;
            var outDownToIdle = (f1 & 0b0100_0000) != 0;
            var outDownToOverDown = (f1 & 0b0010_0000) != 0;
            var overDownToOutDown = (f1 & 0b0001_0000) != 0;
            var overDownToOverUp = (f1 & 0b0000_1000) != 0;
            var overUpToOverDown = (f1 & 0b0000_0100) != 0;
            var overUpToIdle = (f1 & 0b0000_0010) != 0;
            var idleToOverUp = (f1 & 0b0000_0001) != 0;

            // Second octet de flags (key + overDownToIdle)
            var f2 = reader.ReadUi8();
            var keyPress = (f2 >> 1) & 0b0111_1111; // 7 bits
            var overDownToIdle2 = (f2 & 0b0000_0001) != 0;

            var endOfRecord = (size == 0) ? end : start + size;
            var actionRecords = ActionRecord.ReadCollection(reader, endOfRecord);

            list.Add(new ButtonCondAction(
                         size: size,
                         idleToOverDown: idleToOverDown,
                         outDownToIdle: outDownToIdle,
                         outDownToOverDown: outDownToOverDown,
                         overDownToOutDown: overDownToOutDown,
                         overDownToOverUp: overDownToOverUp,
                         overUpToOverDown: overUpToOverDown,
                         overUpToIdle: overUpToIdle,
                         idleToOverUp: idleToOverUp,
                         keyPress: keyPress,
                         overDownToIdle: overDownToIdle2,
                         actions: actionRecords
                     ));
        } while (size != 0 && reader.Offset < end);

        return list;
    }
}