namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Represents a single edge of a shape.
/// </summary>
public interface IEdge
{
    /// <summary>
    /// The X coordinate of the starting point (in twips; 1/20th of a pixel).
    /// </summary>
    int FromX { get; }

    /// <summary>
    /// The Y coordinate of the starting point (in twips; 1/20th of a pixel).
    /// </summary>
    int FromY { get; }

    /// <summary>
    /// The X coordinate of the ending point (in twips; 1/20th of a pixel).
    /// </summary>
    int ToX { get; }

    /// <summary>
    /// The Y coordinate of the ending point (in twips; 1/20th of a pixel).
    /// </summary>
    int ToY { get; }

    /// <summary>
    /// Reverse the edge and return the new instance.
    /// </summary>
    IEdge Reverse();

    /// <summary>
    /// Draw the current edge on the given drawer.
    /// </summary>
    /// <param name="drawer">The path drawer.</param>
    void Draw(IPathDrawer drawer);
}
