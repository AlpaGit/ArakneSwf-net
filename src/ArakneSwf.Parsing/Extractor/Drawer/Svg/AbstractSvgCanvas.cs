using System.Globalization;
using System.Xml.Linq;
using ArakneSwf.Parsing.Extractor.Images;
using ArakneSwf.Parsing.Extractor.Shapes;
using ArakneSwf.Parsing.Extractor.Timelines;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;
using Path = ArakneSwf.Parsing.Extractor.Shapes.Path;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg;

/// <summary>
/// Base implementation for SVG canvas.
/// </summary>
public abstract class AbstractSvgCanvas : IDrawer
{
    /// <summary>
    /// The current drawing root element.
    /// Will be created on first call to <see cref="Area(Rectangle)"/>.
    /// It should be unique for each canvas / drawn object.
    /// </summary>
    private XElement? _currentGroup;

    /// <summary>
    /// The current target group element.
    /// This target depends on the current active clips; each clipping creates a new nested group.
    /// If there is no active clip, this will be the same as <see cref="_currentGroup"/>.
    /// If this value is null, the next drawing will resolve a new target group.
    /// </summary>
    private XElement? _currentTarget;

    /// <summary>
    /// The bounds of the current drawing area.
    /// </summary>
    private Rectangle? _bounds;

    /// <summary>
    /// All active clipPath ids.
    /// </summary>
    private readonly HashSet<string> _activeClipPaths = new();

    private readonly SvgBuilder _builder;

    protected AbstractSvgCanvas(SvgBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    // -----------------------------
    // IDrawer implementation
    // -----------------------------

    /// <summary>
    /// Initializes the drawing area.
    /// </summary>
    public void Area(Rectangle bounds)
    {
        _currentTarget = _currentGroup = NewGroup(_builder, bounds);
        _bounds = bounds;
    }

    /// <summary>
    /// Draws a shape by iterating its paths.
    /// </summary>
    public void Shape(Shape shape)
    {
        _currentTarget = NewGroupWithOffset(_builder, shape.XOffset, shape.YOffset);

        foreach (var path in shape.Paths)
        {
            Path(path);
        }
    }

    /// <summary>
    /// Draws an image element.
    /// </summary>
    public void Image(IImageCharacter image)
    {
        var g = _currentTarget = NewGroup(_builder, image.Bounds());
        var tag = new XElement("image");
        g.Add(tag);

        // xlink:href
        var xlink = SvgBuilder.XLinkNs; // assume: public static readonly XNamespace XLinkNs
        tag.SetAttributeValue(xlink + "href", image.ToBase64Data());
    }

    /// <summary>
    /// Includes a drawable with transform, filters, and optional blend mode/name.
    /// </summary>
    public void Include(
        IDrawable              obj,
        Matrix                 matrix,
        int                    frame     = 0,
        IReadOnlyList<Filter>? filters   = null,
        BlendMode              blendMode = BlendMode.Normal,
        string?                name      = null)
    {
        var included = new IncludedSvgCanvas(this, Defs());
        obj.Draw(included, frame);

        var g = Target(obj.Bounds());
        var width = obj.Bounds().Width() / 20.0;
        var height = obj.Bounds().Height() / 20.0;

        string? filterId = null;
        var enumerable = filters as object[] ?? filters?.ToArray();
        if ((enumerable != null && enumerable.Length != 0) && included.Ids.Any())
        {
            filterId = "filter-" + included.Ids.First();
            _builder.AddFilter(enumerable, filterId, width, height);
        }

        foreach (var id in included.Ids)
        {
            var use = new XElement("use");
            g.Add(use);

            var xlink = SvgBuilder.XLinkNs;
            use.SetAttributeValue(xlink + "href", "#" + id);
            use.SetAttributeValue("width", width.ToString(CultureInfo.InvariantCulture));
            use.SetAttributeValue("height", height.ToString(CultureInfo.InvariantCulture));
            use.SetAttributeValue("transform", matrix.ToSvgTransformation());

            if (!string.IsNullOrEmpty(name))
            {
                use.SetAttributeValue("id", name);
            }

            if (!string.IsNullOrEmpty(filterId))
            {
                use.SetAttributeValue("filter", $"url(#{filterId})");
            }

            var cssBlendMode = blendMode.ToCssValue();
            if (!string.IsNullOrEmpty(cssBlendMode))
            {
                use.SetAttributeValue("style", "mix-blend-mode: " + cssBlendMode);
            }
        }
    }

    /// <summary>
    /// Starts a clipping context and returns the clipPath id.
    /// </summary>
    public string StartClip(IDrawable obj, Matrix matrix, int frame)
    {
        var group = _currentGroup ?? throw new InvalidOperationException("No group defined for clipping");
        var clipPath = new XElement("clipPath");
        group.Add(clipPath);

        var id = NextObjectId();
        clipPath.SetAttributeValue("id", id);
        clipPath.SetAttributeValue("transform", matrix.ToSvgTransformation());

        var clipPathDrawer = new ClipPathBuilder(clipPath, _builder);
        obj.Draw(clipPathDrawer, frame);

        _activeClipPaths.Add(id);

        // Reset the current target so the next drawing applies the clip path on a new group
        _currentTarget = null;

        return id;
    }

    /// <summary>
    /// Ends a clipping context by id.
    /// </summary>
    public void EndClip(string clipId)
    {
        _activeClipPaths.Remove(clipId);

        // Reset the current target so the next drawing applies the clip path on a new group
        _currentTarget = null;
    }

    /// <summary>
    /// Adds a path to the current target group.
    /// </summary>
    public void Path(Path path)
    {
        var g = _currentTarget ?? throw new InvalidOperationException("No group defined");
        _builder.AddPath(g, path);
    }

    public abstract object? Render();


    private XElement Target(Rectangle bounds)
    {
        if (_currentTarget is not null)
            return _currentTarget;

        var rootGroup = _currentGroup ??= NewGroup(_builder, _bounds ?? bounds);

        // No clipping: use the root group
        if (_activeClipPaths.Count == 0)
            return _currentTarget = rootGroup;

        // If there are active clip paths, create nested groups and apply the clip paths
        var target = rootGroup;

        foreach (var id in _activeClipPaths)
        {
            var nested = new XElement("g");
            nested.SetAttributeValue("clip-path", $"url(#{id})");
            target.Add(nested);
            target = nested;
        }

        return _currentTarget = target;
    }

    /// <summary>
    /// Generate a new object id.
    /// Should only be called internally by the canvas.
    /// </summary>
    public abstract string NextObjectId();

    /// <summary>
    /// Get the element storing the definitions of this canvas.
    /// </summary>
    protected abstract XElement Defs();

    /// <summary>
    /// Create a new group element with the given bounds.
    /// Implementation must call <c>SvgBuilder.AddGroup(...)</c>.
    /// </summary>
    protected abstract XElement NewGroup(SvgBuilder builder, Rectangle bounds);

    /// <summary>
    /// Create a new group element with the given offset.
    /// Implementation must call <c>SvgBuilder.AddGroupWithOffset(...)</c>.
    /// </summary>
    protected abstract XElement NewGroupWithOffset(SvgBuilder builder, int offsetX, int offsetY);
}