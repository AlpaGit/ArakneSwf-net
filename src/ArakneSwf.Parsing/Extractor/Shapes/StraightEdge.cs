namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Edge type for straight line segments.
/// Coordinates are in twips (1/20th of a pixel).
/// </summary>
public sealed class StraightEdge : IEdge
{
    public StraightEdge(int fromX, int fromY, int toX, int toY)
    {
        FromX = fromX;
        FromY = fromY;
        ToX = toX;
        ToY = toY;
    }

    public int FromX { get; }
    public int FromY { get; }
    public int ToX { get; }
    public int ToY { get; }

    public IEdge Reverse() => new StraightEdge(ToX, ToY, FromX, FromY);

    public void Draw(IPathDrawer drawer)
    {
        drawer.Line(ToX, ToY);
    }
}