namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>Edge type for quadratic curves.</summary>
public sealed class CurvedEdge : IEdge
{
    public CurvedEdge(int fromX, int fromY, int controlX, int controlY, int toX, int toY)
    {
        FromX = fromX;
        FromY = fromY;
        ControlX = controlX;
        ControlY = controlY;
        ToX = toX;
        ToY = toY;
    }

    // IEdge (required)
    public int FromX { get; }
    public int FromY { get; }
    public int ToX   { get; }
    public int ToY   { get; }

    // Specific to CurvedEdge
    public int ControlX { get; }
    public int ControlY { get; }

    public IEdge Reverse()
        => new CurvedEdge(ToX, ToY, ControlX, ControlY, FromX, FromY);

    public void Draw(IPathDrawer drawer)
    {
        drawer.Curve(ControlX, ControlY, ToX, ToY);
    }
}
