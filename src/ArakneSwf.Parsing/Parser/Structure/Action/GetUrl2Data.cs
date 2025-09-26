namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Données pour l'action GetURL2.
/// </summary>
public sealed class GetUrl2Data
{
    /// <summary>
    /// Méthode d’envoi des variables, codée sur 2 bits (flags[7..6]).
    /// 0 = none, 1 = GET, 2 = POST (selon la spec SWF).
    /// </summary>
    public int SendVarsMethod { get; }

    /// <summary>True si chargement dans une cible (flags bit1).</summary>
    public bool LoadTargetFlag { get; }

    /// <summary>True si chargement des variables (flags bit0).</summary>
    public bool LoadVariablesFlag { get; }

    public GetUrl2Data(int sendVarsMethod, bool loadTargetFlag, bool loadVariablesFlag)
    {
        SendVarsMethod = sendVarsMethod;
        LoadTargetFlag = loadTargetFlag;
        LoadVariablesFlag = loadVariablesFlag;
    }

    public static GetUrl2Data Read(SwfReader reader)
    {
        var flags = reader.ReadUi8();

        var sendVarsMethod    = (flags >> 6) & 0b11;       // bits 7..6
        // 4 bits réservés (bits 5..2) — ignorés/attendus à 0
        var loadTargetFlag    = (flags & 0b0000_0010) != 0; // bit 1
        var loadVariablesFlag = (flags & 0b0000_0001) != 0; // bit 0

        return new GetUrl2Data(sendVarsMethod, loadTargetFlag, loadVariablesFlag);
    }
}
