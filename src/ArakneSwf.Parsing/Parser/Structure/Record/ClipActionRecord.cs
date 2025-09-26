using ArakneSwf.Parsing.Parser.Structure.Action;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// ClipActionRecord : flags d'événements + bloc d'actions associé.
/// Fin de la collection : enregistrement avec tous les flags à 0.
/// </summary>
public sealed class ClipActionRecord
{
    public ClipEventFlags Flags { get; }
    public int Size { get; }

    /// <summary>
    /// Code touche (voir ButtonCondAction.KEY_*), ou null si <see cref="ClipEventFlags.KEY_PRESS"/> n'est pas présent.
    /// </summary>
    public int? KeyCode { get; }

    public IReadOnlyList<ActionRecord> Actions { get; }

    public ClipActionRecord(ClipEventFlags flags, int size, int? keyCode, IReadOnlyList<ActionRecord> actions)
    {
        Flags = flags;
        Size = size;
        KeyCode = keyCode;
        Actions = actions;
    }

    /// <summary>
    /// Lit une collection de <see cref="ClipActionRecord"/> jusqu’au drapeau nul.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="version">Version SWF.</param>
    public static List<ClipActionRecord> ReadCollection(SwfReader reader, int version)
    {
        var records = new List<ClipActionRecord>();

        while (reader.Offset < reader.End)
        {
            var flags = ClipEventFlags.Read(reader, version);
            if (flags.Flags == 0) // fin
                break;

            var size = reader.ReadUi32();
            var actionsEndOffset = reader.Offset + size;

            int? keyCode = flags.Has(ClipEventFlags.KEY_PRESS) ? reader.ReadUi8() : (int?)null;

            var actions = ActionRecord.ReadCollection(reader, (int)actionsEndOffset);

            records.Add(new ClipActionRecord(flags, (int)size, keyCode, actions));
        }

        return records;
    }
}