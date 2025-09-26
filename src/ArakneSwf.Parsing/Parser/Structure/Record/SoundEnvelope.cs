namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Sound envelope point (position in 44.1kHz samples + left/right levels).
/// </summary>
public sealed class SoundEnvelope
{
    public int Pos44 { get; }
    public int LeftLevel { get; }
    public int RightLevel { get; }

    public SoundEnvelope(int pos44, int leftLevel, int rightLevel)
    {
        Pos44 = pos44;
        LeftLevel = leftLevel;
        RightLevel = rightLevel;
    }
}