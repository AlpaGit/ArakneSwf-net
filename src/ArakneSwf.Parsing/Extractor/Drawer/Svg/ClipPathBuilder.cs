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
/// Builder for &lt;clipPath&gt; SVG element.
/// Only shapes and sprites are supported, all other methods are ignored.
/// </summary>
public sealed class ClipPathBuilder : IDrawer
{
    private readonly XElement _clipPath;
    private readonly SvgBuilder _builder;
    private readonly IReadOnlyList<Matrix> _transform;

    public ClipPathBuilder(
        XElement             clipPath,
        SvgBuilder           builder,
        IEnumerable<Matrix>? transform = null)
    {
        _clipPath = clipPath ?? throw new ArgumentNullException(nameof(clipPath));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _transform = (transform ?? []).ToArray();
    }

    // -- IDrawer implementation (no-ops where unsupported) --

    public void Area(Rectangle bounds)
    {
        /* ignored */
    }

    public void Shape(Shape shape)
    {
        foreach (var path in shape.Paths)
        {
            // Assumes: SvgBuilder.AddPath returns the created XElement for the path.
            var element = _builder.AddPath(_clipPath, path);

            var transforms = new List<string>();

            foreach (var matrix in _transform)
            {
                transforms.Add(matrix.ToSvgTransformation());
            }

            // translate(xOffset/20, yOffset/20)
            transforms.Add(string.Format(
                               CultureInfo.InvariantCulture,
                               "translate({0},{1})",
                               shape.XOffset / 20.0,
                               shape.YOffset / 20.0));

            element.SetAttributeValue("transform", string.Join(" ", transforms));
        }
    }

    public void Image(IImageCharacter image)
    {
        /* ignored */
    }

    public void Include(
        IDrawable              obj,
        Matrix                 matrix,
        int                    frame     = 0,
        IReadOnlyList<Filter>? filters   = null,
        BlendMode              blendMode = BlendMode.Normal,
        string?                name      = null)
    {
        // Pass-through draw with an appended transform; all other params are ignored.
        var next = new ClipPathBuilder(_clipPath, _builder, _transform.Concat([matrix]));
        obj.Draw(next, frame);
    }

    public string StartClip(IDrawable obj, Matrix matrix, int frame) => string.Empty;

    public void EndClip(string clipId)
    {
        /* ignored */
    }

    public void Path(Path path)
    {
        /* ignored */
    }

    // If your IDrawer includes Render(), keep this; otherwise remove it.
    public object? Render() => null;
}