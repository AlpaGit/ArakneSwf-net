namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Données du bytecode ActionDefineFunction2 (SWF7).
/// Champs équivalents à la classe PHP fournie.
/// </summary>
public sealed class DefineFunction2Data
{
    public string Name { get; }
    public int RegisterCount { get; }

    public bool PreloadParentFlag { get; }
    public bool PreloadRootFlag { get; }
    public bool SuppressSuperFlag { get; }
    public bool PreloadSuperFlag { get; }
    public bool SuppressArgumentsFlag { get; }
    public bool PreloadArgumentsFlag { get; }
    public bool SuppressThisFlag { get; }
    public bool PreloadThisFlag { get; }
    public bool PreloadGlobalFlag { get; }

    /// <summary>Paramètres (noms) dans l’ordre.</summary>
    public IReadOnlyList<string> Parameters { get; }

    /// <summary>Registres associés aux paramètres (UI8).</summary>
    public IReadOnlyList<int> Registers { get; }

    /// <summary>Taille du bloc de code qui suit (UI16).</summary>
    public int CodeSize { get; }

    public DefineFunction2Data(
        string       name,
        int          registerCount,
        bool         preloadParentFlag,
        bool         preloadRootFlag,
        bool         suppressSuperFlag,
        bool         preloadSuperFlag,
        bool         suppressArgumentsFlag,
        bool         preloadArgumentsFlag,
        bool         suppressThisFlag,
        bool         preloadThisFlag,
        bool         preloadGlobalFlag,
        List<string> parameters,
        List<int>    registers,
        int          codeSize)
    {
        Name = name;
        RegisterCount = registerCount;
        PreloadParentFlag = preloadParentFlag;
        PreloadRootFlag = preloadRootFlag;
        SuppressSuperFlag = suppressSuperFlag;
        PreloadSuperFlag = preloadSuperFlag;
        SuppressArgumentsFlag = suppressArgumentsFlag;
        PreloadArgumentsFlag = preloadArgumentsFlag;
        SuppressThisFlag = suppressThisFlag;
        PreloadThisFlag = preloadThisFlag;
        PreloadGlobalFlag = preloadGlobalFlag;
        Parameters = parameters;
        Registers = registers;
        CodeSize = codeSize;
    }

    /// <summary>
    /// Lecture du bloc DefineFunction2 (même logique que la version PHP).
    /// </summary>
    public static DefineFunction2Data Read(SwfReader reader)
    {
        var functionName = reader.ReadNullTerminatedString();
        int numParams = reader.ReadUi16();
        int registerCount = reader.ReadUi8();

        // Flags sur 16 bits, lus ici en deux octets comme en PHP.
        int flags1 = reader.ReadUi8();
        var preloadParentFlag = (flags1 & 0b1000_0000) != 0;
        var preloadRootFlag = (flags1 & 0b0100_0000) != 0;
        var suppressSuperFlag = (flags1 & 0b0010_0000) != 0;
        var preloadSuperFlag = (flags1 & 0b0001_0000) != 0;
        var suppressArgumentsFlag = (flags1 & 0b0000_1000) != 0;
        var preloadArgumentsFlag = (flags1 & 0b0000_0100) != 0;
        var suppressThisFlag = (flags1 & 0b0000_0010) != 0;
        var preloadThisFlag = (flags1 & 0b0000_0001) != 0;

        // 7 bits réservés dans le second octet ; bit 0 = preloadGlobalFlag
        int flags2 = reader.ReadUi8();
        var preloadGlobalFlag = (flags2 & 0b0000_0001) != 0;

        var parameters = new List<string>(numParams);
        var registers = new List<int>(numParams);

        for (var i = 0; i < numParams; i++)
        {
            registers.Add(reader.ReadUi8());
            parameters.Add(reader.ReadNullTerminatedString());
        }

        int codeSize = reader.ReadUi16();

        return new DefineFunction2Data(
            functionName,
            registerCount,
            preloadParentFlag,
            preloadRootFlag,
            suppressSuperFlag,
            preloadSuperFlag,
            suppressArgumentsFlag,
            preloadArgumentsFlag,
            suppressThisFlag,
            preloadThisFlag,
            preloadGlobalFlag,
            parameters,
            registers,
            codeSize
        );
    }
}