namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// SymbolClass (TYPE = 76)
/// Maps symbol (character) IDs to AS3 class names.
/// </summary>
public sealed class SymbolClassTag
{
    public const int TYPE = 76;

    /// <summary>
    /// Map of symbol id (character id) to symbol name (AS3 class name).
    /// </summary>
    public IReadOnlyDictionary<int, string> Symbols { get; }

    public SymbolClassTag(IReadOnlyDictionary<int, string> symbols)
    {
        Symbols = symbols;
    }

    /// <summary>
    /// Read a SymbolClass tag from the SWF reader.
    /// </summary>
    /// <exception cref="ParserOutOfBoundException" />
    /// <exception cref="ParserInvalidDataException" />
    public static SymbolClassTag Read(SwfReader reader)
    {
        var symbols = new Dictionary<int, string>();
        var count = reader.ReadUi16();

        for (var i = 0; i < count; i++)
        {
            int id = reader.ReadUi16();
            var name = reader.ReadNullTerminatedString();
            symbols[id] = name;
        }

        return new SymbolClassTag(symbols);
    }
}
