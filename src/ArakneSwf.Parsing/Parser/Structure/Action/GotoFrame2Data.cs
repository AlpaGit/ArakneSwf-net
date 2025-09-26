namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Données pour l'action GotoFrame2.
/// </summary>
public sealed class GotoFrame2Data
{
    /// <summary>Bit 1 : indique si un sceneBias (UI16) suit.</summary>
    public bool SceneBiasFlag { get; }

    /// <summary>Bit 0 : indique si la lecture doit démarrer (play).</summary>
    public bool PlayFlag { get; }

    /// <summary>Scene bias (UI16) présent uniquement si <see cref="SceneBiasFlag"/> est vrai.</summary>
    public int? SceneBias { get; }

    public GotoFrame2Data(bool sceneBiasFlag, bool playFlag, int? sceneBias)
    {
        SceneBiasFlag = sceneBiasFlag;
        PlayFlag = playFlag;
        SceneBias = sceneBias;
    }

    public static GotoFrame2Data Read(SwfReader reader)
    {
        var flags = reader.ReadUi8();

        // 6 bits réservés (bits 7..2)
        var sceneBiasFlag = (flags & 0b0000_0010) != 0; // bit 1
        var playFlag      = (flags & 0b0000_0001) != 0; // bit 0

        var sceneBias = sceneBiasFlag ? reader.ReadUi16() : (int?)null;

        return new GotoFrame2Data(sceneBiasFlag, playFlag, sceneBias);
    }
}