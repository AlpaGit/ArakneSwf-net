namespace ArakneSwf.Parsing.Parser.Structure.Record.Shape;

/// <summary>
/// Segment de courbe quadratique d'un tracé SWF.
/// </summary>
public sealed class CurvedEdgeRecord : ShapeRecord
{
    public int ControlDeltaX { get; }
    public int ControlDeltaY { get; }
    public int AnchorDeltaX { get; }
    public int AnchorDeltaY { get; }

    public CurvedEdgeRecord(int controlDeltaX, int controlDeltaY, int anchorDeltaX, int anchorDeltaY)
    {
        ControlDeltaX = controlDeltaX;
        ControlDeltaY = controlDeltaY;
        AnchorDeltaX = anchorDeltaX;
        AnchorDeltaY = anchorDeltaY;
    }
}