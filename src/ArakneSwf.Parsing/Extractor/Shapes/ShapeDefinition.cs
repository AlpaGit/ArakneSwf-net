using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Drawer.Svg;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Store a single shape extracted from a SWF file.
/// </summary>
/// <remarks>
/// See <see cref="DefineShapeTag"/> and <see cref="DefineShape4Tag"/>.
/// </remarks>
public sealed class ShapeDefinition : IDrawable
{
    private Shape? _shape;
    private ShapeProcessor? _processor;

    public ShapeDefinition(ShapeProcessor processor, int id, IDefineShapeTag tag)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        Id = id;
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
    }

    // Private ctor used when returning a transformed copy
    private ShapeDefinition(int id, IDefineShapeTag tag, Shape shape)
    {
        Id = id;
        Tag = tag;
        _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        _processor = null;
    }

    /// <summary>
    /// The character id of the shape.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The raw tag extracted from the SWF file.
    /// </summary>
    public IDefineShapeTag Tag { get; }

    /// <summary>
    /// Get the shape object. The shape is processed once and cached.
    /// </summary>
    public Shape Shape()
    {
        if (_shape == null)
        {
            _shape = _processor!.Process(Tag);
            _processor = null; // free the processor
        }

        return _shape;
    }

    public Rectangle Bounds() => Tag.ShapeBounds;

    public int FramesCount(bool recursive = false) => 1;

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Shape(Shape());
        return drawer;
    }

    /// <summary>
    /// Convert the shape to an SVG string.
    /// </summary>
    public string ToSvg()
    {
        var canvas = new SvgCanvas(Bounds());
        Draw(canvas);
        return canvas.Render();
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        if (colorTransform is null) throw new ArgumentNullException(nameof(colorTransform));
        var transformed = Shape().TransformColors(colorTransform);
        return new ShapeDefinition(Id, Tag, transformed);
    }
}