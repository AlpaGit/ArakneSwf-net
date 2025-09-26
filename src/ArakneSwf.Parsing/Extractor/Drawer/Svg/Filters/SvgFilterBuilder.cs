using System.Globalization;
using System.Xml.Linq;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg.Filters;

/// <summary>
/// Builds SVG filters for Flash-style filters.
/// This class is stateful and directly mutates the provided &lt;filter&gt; element.
/// Recreate per filter chain.
/// </summary>
public sealed class SvgFilterBuilder
{
    private int _filterCount = 0;
    private string _lastResult = "SourceGraphic";
    private double _xOffset = 0;
    private double _yOffset = 0;

    private readonly XElement _filter;
    private readonly double _width;
    private readonly double _height;

    private SvgFilterBuilder(XElement filter, double width, double height)
    {
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        _width = width;
        _height = height;
    }

    /// <summary>
    /// Apply a new filter to the current builder.
    /// Updates the internal "last result" id for chaining.
    /// </summary>
    public void Apply(object filter)
    {
        if (filter is null) throw new ArgumentNullException(nameof(filter));

        _lastResult = filter switch
        {
            ColorMatrixFilter cm => SvgColorMatrixFilter.Apply(this, cm, _lastResult),
            BlurFilter blur      => SvgBlurFilter.Apply(this, blur, _lastResult),
            GlowFilter glow      => SvgGlowFilter.Apply(this, glow, _lastResult),
            DropShadowFilter ds  => SvgDropShadowFilter.Apply(this, ds, _lastResult),

            // Add cases as you implement them:
            // BevelFilter, GradientGlowFilter, ConvolutionFilter, GradientBevelFilter
            _ => throw new InvalidOperationException($"Unsupported filter type: {filter.GetType().FullName}")
        };
    }

    /// <summary>
    /// Create a new filter primitive element (e.g., feGaussianBlur). Optionally sets the 'in' attribute.
    /// </summary>
    public XElement AddFilter(string element, string? input = null)
    {
        var el = new XElement(element);
        _filter.Add(el);

        if (!string.IsNullOrEmpty(input))
            el.SetAttributeValue("in", input);

        return el;
    }

    /// <summary>
    /// Create a new filter primitive element and assign a unique 'result' (and 'id') to it.
    /// Returns the element and the result id.
    /// </summary>
    public (XElement element, string resultId) AddResultFilter(string element, string? input = null)
    {
        var el = AddFilter(element, input);
        var result = "filter" + (++_filterCount).ToString(CultureInfo.InvariantCulture);

        el.SetAttributeValue("result", result);
        el.SetAttributeValue("id", result);

        return (el, result);
    }

    /// <summary>
    /// Increase the filter region offsets (expands width/height and shifts x/y outward).
    /// </summary>
    public void AddOffset(double x, double y)
    {
        _xOffset += x;
        _yOffset += y;
    }

    /// <summary>
    /// Apply computed properties to the &lt;filter&gt; element (must be called after all primitives are added).
    /// </summary>
    public void FinalizeBuild()
    {
        if (_xOffset > 0 || _yOffset > 0)
        {
            _filter.SetAttributeValue("width",
                                      (_width + _xOffset * 2).ToString(CultureInfo.InvariantCulture));
            _filter.SetAttributeValue("height",
                                      (_height + _yOffset * 2).ToString(CultureInfo.InvariantCulture));
            _filter.SetAttributeValue("x",
                                      (-_xOffset).ToString(CultureInfo.InvariantCulture));
            _filter.SetAttributeValue("y",
                                      (-_yOffset).ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Factory: create a new &lt;filter&gt; element under <paramref name="root"/> and return a builder for it.
    /// </summary>
    public static SvgFilterBuilder Create(XElement root, string id, double width, double height)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));
        if (id is null) throw new ArgumentNullException(nameof(id));

        var filter = new XElement("filter");
        root.Add(filter);

        filter.SetAttributeValue("id", id);
        filter.SetAttributeValue("filterUnits", "userSpaceOnUse"); // Allow overflow

        return new SvgFilterBuilder(filter, width, height);
    }

    // (Optional) expose last result if needed by callers chaining external primitives.
    public string LastResult => _lastResult;

    // (Optional) expose the underlying <filter> node if needed.
    public XElement Element => _filter;
}