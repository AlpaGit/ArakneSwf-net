namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// ExportAssets tag (ID = 56).
/// Maps exported character IDs to their linkage names.
/// </summary>
public sealed class ExportAssetsTag
{
    public const int ID = 56;

    /// <summary>
    /// Map of exported character IDs to their names.
    /// </summary>
    public IReadOnlyDictionary<int, string> Characters { get; }

    public ExportAssetsTag(IReadOnlyDictionary<int, string> characters)
    {
        Characters = characters;
    }

    /// <summary>
    /// Read an ExportAssets tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    public static ExportAssetsTag Read(SwfReader reader)
    {
        var characters = new Dictionary<int, string>();
        int count = reader.ReadUi16();

        for (var i = 0; i < count && reader.Offset < reader.End; i++)
        {
            int id = reader.ReadUi16();
            var name = reader.ReadNullTerminatedString();
            characters[id] = name;
        }

        return new ExportAssetsTag(characters);
    }
}
