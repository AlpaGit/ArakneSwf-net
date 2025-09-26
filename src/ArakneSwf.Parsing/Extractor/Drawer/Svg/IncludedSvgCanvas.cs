using System.Xml.Linq;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg;

public sealed class IncludedSvgCanvas : AbstractSvgCanvas
{
    /// <summary>
    /// List of ids of objects drawn in this canvas.
    /// Each id should be referenced with a &lt;use&gt; tag.
    /// </summary>
    private readonly List<string> _ids = new();

    public IReadOnlyList<string> Ids => _ids;

    private readonly AbstractSvgCanvas _root;
    private readonly XElement _defs;

    /// <param name="root">The root canvas</param>
    /// <param name="defs">The &lt;defs&gt; element of the root canvas</param>
    public IncludedSvgCanvas(AbstractSvgCanvas root, XElement defs)
        : base(new SvgBuilder(defs))
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
        _defs = defs ?? throw new ArgumentNullException(nameof(defs));
    }

    /// <summary>
    /// Rendering is performed by the root canvas.
    /// </summary>
    public override string Render()
        => throw new InvalidOperationException(
            "This is an internal implementation, rendering is performed by the root canvas");

    /// <summary>
    /// Delegate id generation to the root so ids remain unique within the document.
    /// NOTE: Requires AbstractSvgCanvas.NextObjectId() to be accessible here
    /// (e.g., declared as 'protected internal' in the base class).
    /// </summary>
    public override string NextObjectId()
    {
        return _root.NextObjectId();
    }

    protected override XElement Defs() => _defs;

    protected override XElement NewGroup(SvgBuilder builder, Rectangle bounds)
    {
        var group = builder.AddGroup(bounds);
        var id = NextObjectId();
        _ids.Add(id);
        group.SetAttributeValue("id", id);
        return group;
    }

    protected override XElement NewGroupWithOffset(SvgBuilder builder, int offsetX, int offsetY)
    {
        var group = builder.AddGroupWithOffset(offsetX, offsetY);
        var id = NextObjectId();
        _ids.Add(id);
        group.SetAttributeValue("id", id);
        return group;
    }
}