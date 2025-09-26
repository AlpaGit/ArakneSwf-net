namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Zone data entry (alignment coordinate and range).
/// </summary>
public sealed class ZoneData
{
    public float AlignmentCoordinate { get; }
    public float Range { get; }

    public ZoneData(float alignmentCoordinate, float range)
    {
        AlignmentCoordinate = alignmentCoordinate;
        Range = range;
    }
}