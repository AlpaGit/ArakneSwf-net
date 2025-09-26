namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Draw a single path.
/// Implementations are stateful and should be used only once.
/// Coordinates are in twips (1/20th of a pixel).
/// </summary>
public interface IPathDrawer
{
    /// <summary>Move the cursor to the given position.</summary>
    void Move(int x, int y);

    /// <summary>Draw a line from the current cursor position to the given position, then update the cursor.</summary>
    void Line(int toX, int toY);

    /// <summary>Draw a curve from the current cursor position to the given position, then update the cursor.</summary>
    void Curve(int controlX, int controlY, int toX, int toY);

    /// <summary>Finalize the path and draw it.</summary>
    void Draw();
}