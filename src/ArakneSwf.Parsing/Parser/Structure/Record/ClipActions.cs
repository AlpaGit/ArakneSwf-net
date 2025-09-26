namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Conteneur d’actions de clip (flags globaux + enregistrements).
/// </summary>
public sealed class ClipActions
{
    public ClipEventFlags AllEventFlags { get; }
    public IReadOnlyList<ClipActionRecord> Records { get; }

    public ClipActions(ClipEventFlags allEventFlags, IReadOnlyList<ClipActionRecord> records)
    {
        AllEventFlags = allEventFlags;
        Records = records;
    }

    /// <summary>
    /// Lit une structure <see cref="ClipActions"/> depuis le flux SWF.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="version">Version SWF.</param>
    public static ClipActions Read(SwfReader reader, int version)
    {
        reader.SkipBytes(2); // UI16 réservé, doit être 0

        var allFlags = ClipEventFlags.Read(reader, version);
        var records = ClipActionRecord.ReadCollection(reader, version);

        return new ClipActions(allFlags, records);
    }
}