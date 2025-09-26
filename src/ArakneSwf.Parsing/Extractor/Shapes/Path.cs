using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Structure for a polygon or line path.
/// NOTE: this type is mutable; <see cref="Push"/> mutates the current instance.
/// </summary>
public sealed class Path
{
    private List<IEdge> _edges;

    public Path(IEnumerable<IEdge> edges, PathStyle style)
    {
        if (edges is null) throw new ArgumentNullException(nameof(edges));
        Style = style ?? throw new ArgumentNullException(nameof(style));
        _edges = new List<IEdge>(edges);
    }

    public PathStyle Style { get; }

    /// <summary>
    /// Push new edges at the end of the path (mutates this instance).
    /// </summary>
    public Path Push(params IList<IEdge> edges)
    {
        if (edges is null) return this;
        foreach (var e in edges)
        {
            if (e is null) continue;
            _edges.Add(e);
        }

        return this;
    }

    /// <summary>
    /// Try to reconnect edges that are not connected. The order of edges may change.
    /// Returns a new <see cref="Path"/> instance; this object is not modified.
    /// </summary>
    public Path Fix()
    {
        var remaining = new List<IEdge>(_edges);
        var fixedEdges = new List<IEdge>(remaining.Count);

        while (remaining.Count > 0)
        {
            // 2. Pop the first edge from the set
            var current = remaining[0];
            remaining.RemoveAt(0);
            fixedEdges.Add(current);

            // 4-6. Find next connected edge; if none, break to start a new disconnected chain
            while (true)
            {
                var found = false;

                for (int i = 0; i < remaining.Count; i++)
                {
                    var other = remaining[i];

                    // Case 1: current.to == other.from
                    if (current.ToX == other.FromX && current.ToY == other.FromY)
                    {
                        fixedEdges.Add(other);
                        remaining.RemoveAt(i);
                        current = other;
                        found = true;
                        break;
                    }

                    // Case 2: current.to == other.to  => reverse the other edge
                    if (current.ToX == other.ToX && current.ToY == other.ToY)
                    {
                        var reversed = other.Reverse();
                        fixedEdges.Add(reversed);
                        remaining.RemoveAt(i);
                        current = reversed;
                        found = true;
                        break;
                    }
                }

                if (!found) break;
            }
        }

        return new Path(fixedEdges, Style);
    }

    /// <summary>
    /// Draw the current path.
    /// </summary>
    public void Draw(IPathDrawer drawer)
    {
        int? lastX = null;
        int? lastY = null;

        foreach (var edge in _edges)
        {
            if (!lastX.HasValue || edge.FromX != lastX.Value || edge.FromY != lastY.Value)
            {
                drawer.Move(edge.FromX, edge.FromY);
            }

            edge.Draw(drawer);

            lastX = edge.ToX;
            lastY = edge.ToY;
        }

        drawer.Draw();
    }

    public Path TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));
        // Copy the edge list to avoid accidental sharing
        return new Path(new List<IEdge>(_edges), Style.TransformColors(colorTransform));
    }
}