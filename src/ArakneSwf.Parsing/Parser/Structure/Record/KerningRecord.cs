namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Kerning pair: (code1, code2) → adjustment.
/// </summary>
public sealed class KerningRecord
{
    public int Code1 { get; }
    public int Code2 { get; }
    public int Adjustment { get; }

    public KerningRecord(int code1, int code2, int adjustment)
    {
        Code1 = code1;
        Code2 = code2;
        Adjustment = adjustment;
    }
}
