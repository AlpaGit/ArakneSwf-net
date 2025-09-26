using System.Globalization;
using System.Xml.Linq;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg;

/// <summary>
/// Drawer implementation to generate SVG XML.
/// </summary>
public sealed class SvgCanvas : AbstractSvgCanvas
{
    private static readonly XNamespace SvgNs   = "http://www.w3.org/2000/svg";
    private static readonly XNamespace XlinkNs = "http://www.w3.org/1999/xlink";

    private readonly XElement _root;
    private XElement? _defs;
    private int _lastId = 0;

    // Public ctor that builds the root <svg> and forwards it to the base via a private ctor.
    public SvgCanvas(Rectangle bounds)
        : this(CreateRootSvg(bounds)) { }

    // Private ctor to allow us to pass the root to the base-class SvgBuilder before the body runs.
    private SvgCanvas(XElement root)
        : base(new SvgBuilder(root))
    {
        _root = root;
    }

    /// <summary>
    /// Render the SVG as an XML string.
    /// (If your base class defines a virtual/abstract Render(), mark this as an override.)
    /// </summary>
    public override string Render() => ToXml();

    /// <summary>
    /// Render the SVG as XML.
    /// </summary>
    public string ToXml()
    {

        // Create a document wrapper so namespaces are emitted cleanly.
        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), _root);
        return doc.ToString(SaveOptions.DisableFormatting).Replace("xmlns=\"\"", "");
    }

    // ---- AbstractSvgCanvas hooks ----

    public override string NextObjectId() => $"object-{_lastId++}";

    protected override XElement Defs()
    {
        if (_defs is null)
        {
            _defs = new XElement(SvgNs + "defs");
            _root.Add(_defs);
        }

        return _defs;
    }

    protected override XElement NewGroup(SvgBuilder builder, Rectangle bounds)
        => builder.AddGroup(bounds);

    protected override XElement NewGroupWithOffset(SvgBuilder builder, int offsetX, int offsetY)
        => builder.AddGroupWithOffset(offsetX, offsetY);

    // ---- helpers ----

    private static XElement CreateRootSvg(Rectangle bounds)
    {

        var root = new XElement(
            SvgNs + "svg",
            // Declare xlink prefix
            new XAttribute(XNamespace.Xmlns + "xlink", XlinkNs.NamespaceName)
        );

        root.SetAttributeValue(
            "width",
            (bounds.Width() / 20.0).ToString(CultureInfo.InvariantCulture) + "px");
        root.SetAttributeValue(
            "height",
            (bounds.Height() / 20.0).ToString(CultureInfo.InvariantCulture) + "px");

        return root;
    }
}