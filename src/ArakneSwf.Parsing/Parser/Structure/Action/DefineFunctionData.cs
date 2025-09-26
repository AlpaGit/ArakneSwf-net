namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Données du bytecode ActionDefineFunction (SWF5).
/// </summary>
public sealed class DefineFunctionData
{
    public string Name { get; }
    public IReadOnlyList<string> Parameters { get; }

    /// <summary>Taille du bloc de code qui suit (UI16).</summary>
    public int CodeSize { get; }

    public DefineFunctionData(string name, List<string> parameters, int codeSize)
    {
        Name = name;
        Parameters = parameters;
        CodeSize = codeSize;
    }

    /// <summary>
    /// Lecture du bloc DefineFunction (même logique que la version PHP).
    /// </summary>
    public static DefineFunctionData Read(SwfReader reader)
    {
        var name = reader.ReadNullTerminatedString();
        int numParams = reader.ReadUi16();

        var parameters = new List<string>(numParams);
        for (var i = 0; i < numParams; i++)
        {
            parameters.Add(reader.ReadNullTerminatedString());
        }

        int codeSize = reader.ReadUi16();

        return new DefineFunctionData(name, parameters, codeSize);
    }
}