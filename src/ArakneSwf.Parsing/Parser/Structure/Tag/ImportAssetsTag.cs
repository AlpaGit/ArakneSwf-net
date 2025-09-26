namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// ImportAssets (v1) / ImportAssets2 (v2) tag.
/// </summary>
public sealed class ImportAssetsTag
{
    public const int TYPE_V1 = 57;
    public const int TYPE_V2 = 71;

    /// <summary>1 for ImportAssets, 2 for ImportAssets2.</summary>
    public int Version { get; }

    /// <summary>Path or URL of the SWF to import.</summary>
    public string Url { get; }

    /// <summary>Map of character IDs (in this SWF) to exported names (in the source SWF).</summary>
    public IReadOnlyDictionary<int, string> Characters { get; }

    public ImportAssetsTag(int version, string url, IReadOnlyDictionary<int, string> characters)
    {
        Version = version;
        Url = url;
        Characters = characters;
    }

    /// <summary>
    /// Read an ImportAssets (v1) or ImportAssets2 (v2) tag from the reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="version">1 or 2.</param>
    public static ImportAssetsTag Read(SwfReader reader, int version)
    {
        var url = reader.ReadNullTerminatedString();

        if (version == 2)
        {
            reader.SkipBytes(1); // Reserved, must be 1
            reader.SkipBytes(1); // Reserved, must be 0
        }

        var characters = new Dictionary<int, string>();
        int count = reader.ReadUi16();

        for (var i = 0; i < count; i++)
        {
            int id = reader.ReadUi16();
            var name = reader.ReadNullTerminatedString();
            characters[id] = name;
        }

        return new ImportAssetsTag(version, url, characters);
    }
}