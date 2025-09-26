using System.Collections.ObjectModel;
using System.Text;
using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Parser.Structure;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser;

/// <summary>
/// Façade pour parser et extraire des données d’un fichier SWF.
/// </summary>
public sealed class Swf
{
    private readonly SwfReader _reader;

    /// <summary>En-tête SWF.</summary>
    public SwfHeader Header { get; }

    /// <summary>Toutes les balises contenues dans le fichier.</summary>
    public IReadOnlyList<SwfTag> Tags { get; }

    /// <summary>Index des balises DefineXXX par Character ID.</summary>
    public IReadOnlyDictionary<int, SwfTag> Dictionary { get; }

    private Swf(
        SwfReader               reader,
        SwfHeader               header,
        List<SwfTag>            tags,
        Dictionary<int, SwfTag> dictionary)
    {
        _reader = reader;
        Header = header;
        Tags = new ReadOnlyCollection<SwfTag>(tags);
        Dictionary = new ReadOnlyDictionary<int, SwfTag>(dictionary);
    }

    /// <summary>
    /// Parse la donnée d’un tag donné.
    /// Équivalent à <c>$tag->parse($this->reader, $this->header->version)</c>.
    /// </summary>
    public object Parse(SwfTag tag)
        => tag.Parse(_reader, Header.Version);

    /// <summary>
    /// Crée un <see cref="Swf"/> à partir de données binaires en mémoire.
    /// </summary>
    public static Swf FromBytes(byte[] data, Errors errors = Errors.All)
    {
        var reader = new SwfReader(data, errors: errors);
        return Read(reader);
    }

    /// <summary>
    /// Crée un <see cref="Swf"/> à partir d’un lecteur déjà positionné en début de fichier.
    /// </summary>
    public static Swf Read(SwfReader reader)
    {
        // Signature (3 octets)
        var sigBytes = reader.ReadBytes(3);
        var signature = Encoding.ASCII.GetString(sigBytes);

        bool compressed;
        if (signature == "CWS")
            compressed = true;
        else if (signature == "FWS")
            compressed = false;
        else
            throw new ArgumentException($"Unsupported SWF signature: {signature}", nameof(reader));

        int version = reader.ReadUi8();      // byte -> int
        var fileLength = reader.ReadUi32(); // longueur totale annoncée

        if (fileLength < 8)
            throw new ArgumentException($"Invalid SWF file length: {fileLength}", nameof(reader));

        if (compressed)
        {
            // Décompresse la suite (à partir de l’offset courant) jusqu’à fileLength
            reader = reader.Uncompress((int)fileLength);
        }

        var frameSize = Rectangle.Read(reader);
        var frameRate = reader.ReadFixed8();
        var frameCount = reader.ReadUi16();

        var header = new SwfHeader(
            signature,
            version,
            fileLength,
            frameSize,
            frameRate,
            frameCount
        );

        var tags = new List<SwfTag>();
        var dictionary = new Dictionary<int, SwfTag>();

        foreach (var tag in SwfTag.ReadAll(reader))
        {
            tags.Add(tag);
            if (tag.Id.HasValue)
                dictionary[tag.Id.Value] = tag;
        }

        return new Swf(reader, header, tags, dictionary);
    }
}