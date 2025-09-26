namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Build paths of a shape. This builder associates styles to paths and merges them when possible.
/// </summary>
public sealed class PathsBuilder
{
    /// <summary>
    /// Paths that are in the process of being built, keyed by style hash.
    /// </summary>
    private readonly Dictionary<string, Path> _openPaths = new();

    /// <summary>
    /// All paths that have been built (no more edges can be added).
    /// Note: not yet finalized (not fixed nor ordered).
    /// </summary>
    private readonly List<Path> _closedPaths = new();

    /// <summary>
    /// All paths ready to be exported (already fixed and ordered).
    /// </summary>
    private readonly List<Path> _finalizedPaths = new();

    /// <summary>
    /// Active styles used to draw paths. A null entry means "ignore".
    /// </summary>
    private List<PathStyle?> _activeStyles = new();

    /// <summary>
    /// Set active styles that will be used to draw paths.
    /// Passing <c>null</c> entries means the corresponding style is ignored.
    /// </summary>
    public void SetActiveStyles(params PathStyle?[] styles)
    {
        _activeStyles = styles?.ToList() ?? new List<PathStyle?>();
    }

    /// <summary>
    /// Merge new edges into all open paths that match the active styles.
    /// </summary>
    public void Merge(params IList<IEdge> edges)
    {
        if (edges is null || edges.Count == 0) return;

        foreach (var style in _activeStyles)
        {
            if (style is null) continue;

            var toPush = style.Reverse ? ReverseEdges(edges) : edges;
            var key = style.Hash();

            if (!_openPaths.TryGetValue(key, out var lastPath))
            {
                _openPaths[key] = new Path(toPush, style);
            }
            else
            {
                _openPaths[key] = lastPath.Push(toPush);
            }
        }
    }

    /// <summary>
    /// Close all active paths. Should be called when the drawing context changes (e.g., new styles).
    /// Note: automatically called by <see cref="Export"/>.
    /// </summary>
    public void Close()
    {
        if (_openPaths.Count == 0) return;

        foreach (var path in _openPaths.Values)
            _closedPaths.Add(path);

        _openPaths.Clear();
    }

    /// <summary>
    /// Finalize drawing of all active paths and start a new drawing context.
    /// (Named <c>FinalizePaths</c> to avoid clashing with the .NET finalizer.)
    /// </summary>
    public void FinalizePaths()
    {
        var exported = Export();
        _finalizedPaths.Clear();
        _finalizedPaths.AddRange(exported);
        _closedPaths.Clear();
    }

    /// <summary>
    /// Export all built paths (fixed and ordered: fills first, then lines).
    /// </summary>
    public List<Path> Export()
    {
        Close();

        var fillPaths = new List<Path>();
        var linePaths = new List<Path>();

        foreach (var path in _closedPaths)
        {
            var fixedPath = path.Fix();

            if (fixedPath.Style.LineWidth > 0)
                linePaths.Add(fixedPath);
            else
                fillPaths.Add(fixedPath);
        }

        // Line paths should be drawn after fill paths
        return _finalizedPaths.Concat(fillPaths).Concat(linePaths).ToList();
    }

    /// <summary>
    /// Reverse edges, and reverse their order.
    /// </summary>
    private static IEdge[] ReverseEdges(IList<IEdge> edges)
    {
        var reversed = new IEdge[edges.Count];
        int ri = 0;

        for (int i = edges.Count - 1; i >= 0; i--)
            reversed[ri++] = edges[i].Reverse();

        return reversed;
    }
}