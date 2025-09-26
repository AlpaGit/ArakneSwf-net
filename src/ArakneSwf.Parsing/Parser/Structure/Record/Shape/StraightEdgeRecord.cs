namespace ArakneSwf.Parsing.Parser.Structure.Record.Shape;

/// <summary>
/// Segment de ligne droite d'un tracé SWF.
/// </summary>
public sealed class StraightEdgeRecord : ShapeRecord
{
    public bool GeneralLineFlag   { get; }
    public bool VerticalLineFlag  { get; }
    public int  DeltaX            { get; }
    public int  DeltaY            { get; }

    public StraightEdgeRecord(bool generalLineFlag, bool verticalLineFlag, int deltaX, int deltaY)
    {
        GeneralLineFlag  = generalLineFlag;
        VerticalLineFlag = verticalLineFlag;
        DeltaX           = deltaX;
        DeltaY           = deltaY;
    }
}
