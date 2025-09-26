using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using ArakneSwf.Parsing.Extractor.Drawer.Svg.Filters;
using ArakneSwf.Parsing.Extractor.Shapes.FillTypes;
using ArakneSwf.Parsing.Parser.Structure.Record;
using Path = ArakneSwf.Parsing.Extractor.Shapes.Path;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg;

/// <summary>
/// Helper to build SVG elements.
/// </summary>
public sealed class SvgBuilder
{
    // xlink namespace
    public static readonly XNamespace XLinkNs = "http://www.w3.org/1999/xlink";
    private static readonly XNamespace SvgNs  = "http://www.w3.org/2000/svg";

    // Cache of created elements by id (gradients, patterns, images, etc.)
    private readonly Dictionary<string, XElement> _elementsById = new();

    /// <summary>
    /// The SVG element to draw on (root or &lt;defs&gt;).
    /// </summary>
    private readonly XElement _svg;

    public SvgBuilder(XElement svg)
    {
        _svg = svg ?? throw new ArgumentNullException(nameof(svg));
    }

    public XElement AddGroup(Rectangle bounds)
    {
        return AddGroupWithOffset(-bounds.XMin, -bounds.YMin);
    }

    public XElement AddGroupWithOffset(int xOffset, int yOffset)
    {
        var g = new XElement(SvgNs + "g");
        _svg.Add(g);

        var tx = (xOffset / 20.0).ToString(CultureInfo.InvariantCulture);
        var ty = (yOffset / 20.0).ToString(CultureInfo.InvariantCulture);

        g.SetAttributeValue("transform", $"matrix(1, 0, 0, 1, {tx}, {ty})");
        return g;
    }

    public XElement AddPath(XElement g, Path path)
    {
        var pathElement = new XElement(SvgNs + "path");
        g.Add(pathElement);

        // Fill
        ApplyFillStyle(pathElement, path.Style.Fill, "fill");

        // Stroke (line)
        if (path.Style.LineFill is not null)
        {
            ApplyFillStyle(pathElement, path.Style.LineFill, "stroke");
        }
        else
        {
            pathElement.SetAttributeValue("stroke", path.Style.LineColor?.Hex() ?? "none");

            if (path.Style.LineColor?.HasTransparency() == true)
            {
                pathElement.SetAttributeValue("stroke-opacity",
                                              path.Style.LineColor.Opacity().ToString(CultureInfo.InvariantCulture));
            }
        }

        if (path.Style.LineWidth > 0)
        {
            pathElement.SetAttributeValue("stroke-width", Math.Max(1, path.Style.LineWidth / 20.0).ToString(CultureInfo.InvariantCulture));
            pathElement.SetAttributeValue("stroke-linecap", "round"); // TODO: map LINESTYLE2 if available
            pathElement.SetAttributeValue("stroke-linejoin", "round");
        }

        path.Draw(new SvgPathDrawer(pathElement));
        return pathElement;
    }

    /// <summary>
    /// Apply an SVG filter definition into &lt;defs&gt; and reference it by id.
    /// </summary>
    public void AddFilter(IEnumerable<object> filters, string id, double width, double height)
    {
        var builder = SvgFilterBuilder.Create(_svg, id, width, height);

        foreach (var filter in filters)
            builder.Apply(filter);

        builder.FinalizeBuild();
    }

    /// <summary>
    /// Applies a fill or stroke style to a path element.
    /// </summary>
    public void ApplyFillStyle(XElement path, object? style, string attribute)
    {
        if (style is null)
        {
            path.SetAttributeValue(attribute, "none");
            return;
        }

        if (attribute == "fill")
            path.SetAttributeValue("fill-rule", "evenodd");

        switch (style)
        {
            case Solid solid:
                ApplyFillSolid(path, solid, attribute);
                break;

            case LinearGradient linear:
                ApplyFillLinearGradient(path, linear, attribute);
                break;

            case RadialGradient radial:
                ApplyFillRadialGradient(path, radial, attribute);
                break;

            case Bitmap bitmap:
                ApplyFillClippedBitmap(path, bitmap, attribute);
                break;

            default:
                throw new ArgumentException($"Unsupported style type: {style.GetType().Name}", nameof(style));
        }
    }

    public void ApplyFillSolid(XElement path, Solid style, string attribute)
    {
        path.SetAttributeValue(attribute, style.Color.Hex());

        if (style.Color.HasTransparency())
        {
            path.SetAttributeValue($"{attribute}-opacity",
                                   style.Color.Opacity().ToString(CultureInfo.InvariantCulture));
        }
    }

    public void ApplyFillLinearGradient(XElement path, LinearGradient style, string attribute)
    {
        var id = "gradient-" + style.Hash();
        path.SetAttributeValue(attribute, $"url(#{id})");

        if (_elementsById.ContainsKey(id))
            return;

        var linearGradient = new XElement(SvgNs + "linearGradient");
        _svg.Add(linearGradient);
        _elementsById[id] = linearGradient;

        linearGradient.SetAttributeValue("gradientTransform", style.Matrix.ToSvgTransformation());
        linearGradient.SetAttributeValue("gradientUnits", "userSpaceOnUse");
        linearGradient.SetAttributeValue("spreadMethod", "pad");
        linearGradient.SetAttributeValue("id", id);

        // Gradient square: (-16384,-16384) to (16384,16384) → x from -819.2 to 819.2
        linearGradient.SetAttributeValue("x1", "-819.2");
        linearGradient.SetAttributeValue("x2", "819.2");

        foreach (var record in style.Gradient.Records)
        {
            var stop = new XElement(SvgNs + "stop");
            linearGradient.Add(stop);

            stop.SetAttributeValue("offset",
                                   (record.Ratio / 255.0).ToString(CultureInfo.InvariantCulture));
            stop.SetAttributeValue("stop-color", record.Color.Hex());
            stop.SetAttributeValue("stop-opacity",
                                   record.Color.Opacity().ToString(CultureInfo.InvariantCulture));
        }
    }

    public void ApplyFillRadialGradient(XElement path, RadialGradient style, string attribute)
    {
        var id = "gradient-" + style.Hash();
        path.SetAttributeValue(attribute, $"url(#{id})");

        if (_elementsById.ContainsKey(id))
            return;

        var radialGradient = new XElement(SvgNs + "radialGradient");
        _svg.Add(radialGradient);
        _elementsById[id] = radialGradient;

        radialGradient.SetAttributeValue("gradientTransform", style.Matrix.ToSvgTransformation());
        radialGradient.SetAttributeValue("gradientUnits", "userSpaceOnUse");
        radialGradient.SetAttributeValue("spreadMethod", "pad");
        radialGradient.SetAttributeValue("id", id);

        // Gradient square centered at (0,0).
        radialGradient.SetAttributeValue("cx", "0");
        radialGradient.SetAttributeValue("cy", "0");
        radialGradient.SetAttributeValue("r", "819.2");

        if (style.Gradient.FocalPoint is { } fp)
        {
            radialGradient.SetAttributeValue("fx", "0");
            radialGradient.SetAttributeValue("fy",
                                             (fp * 819.2).ToString(CultureInfo.InvariantCulture));
        }

        foreach (var record in style.Gradient.Records)
        {
            var stop = new XElement(SvgNs + "stop");
            radialGradient.Add(stop);

            stop.SetAttributeValue("offset",
                                   (record.Ratio / 255.0).ToString(CultureInfo.InvariantCulture));
            stop.SetAttributeValue("stop-color", record.Color.Hex());

            if (record.Color.HasTransparency())
            {
                stop.SetAttributeValue("stop-opacity",
                                       record.Color.Opacity().ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public void ApplyFillClippedBitmap(XElement path, Bitmap style, string attribute)
    {
        var id = "pattern-" + style.Hash();
        path.SetAttributeValue(attribute, $"url(#{id})");

        if (_elementsById.ContainsKey(id))
            return;

        var pattern = new XElement(SvgNs + "pattern");
        _svg.Add(pattern);
        _elementsById[id] = pattern;

        pattern.SetAttributeValue("id", id);
        pattern.SetAttributeValue("overflow", "visible");
        pattern.SetAttributeValue("patternUnits", "userSpaceOnUse");

        var w = style.BitmapRef.Bounds().Width() / 20.0;
        var h = style.BitmapRef.Bounds().Height() / 20.0;

        pattern.SetAttributeValue("width", w.ToString(CultureInfo.InvariantCulture));
        pattern.SetAttributeValue("height", h.ToString(CultureInfo.InvariantCulture));
        pattern.SetAttributeValue("viewBox",
                                  $"0 0 {w.ToString(CultureInfo.InvariantCulture)} {h.ToString(CultureInfo.InvariantCulture)}");

        pattern.SetAttributeValue("patternTransform",
                                  style.Matrix.ToSvgTransformation(undoTwipScale: true));

        if (!style.Smoothed)
        {
            pattern.SetAttributeValue("image-rendering", "optimizeSpeed");
        }

        var b64 = style.BitmapRef.ToBase64Data();
        var imageId = "image-" + Md5Hex(b64);

        if (!_elementsById.ContainsKey(imageId))
        {
            var image = new XElement(SvgNs + "image");
            pattern.Add(image);
            image.SetAttributeValue(XLinkNs + "href", b64);
            image.SetAttributeValue("id", imageId);
            _elementsById[imageId] = image;
        }
        else
        {
            var use = new XElement(SvgNs + "use");
            pattern.Add(use);
            use.SetAttributeValue(XLinkNs + "href", "#" + imageId);
        }
    }

    // -------- helpers --------

    private static string Md5Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = MD5.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return sb.ToString();
    }
}
