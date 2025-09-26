using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Shape;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineShape4 tag (TYPE = 83).
/// </summary>
public sealed class DefineShape4Tag : IDefineShapeTag
{
    public const int TYPE_V4 = 83;

    public int ShapeId { get; }
    public Rectangle ShapeBounds { get; }
    public Rectangle EdgeBounds { get; }

    /// <summary>5-bit reserved field read from the bitstream.</summary>
    public int Reserved { get; }

    public bool UsesFillWindingRule { get; }
    public bool UsesNonScalingStrokes { get; }
    public bool UsesScalingStrokes { get; }
    public ShapeWithStyle Shapes { get; }

    public DefineShape4Tag(
        int            shapeId,
        Rectangle      shapeBounds,
        Rectangle      edgeBounds,
        int            reserved,
        bool           usesFillWindingRule,
        bool           usesNonScalingStrokes,
        bool           usesScalingStrokes,
        ShapeWithStyle shapes)
    {
        ShapeId = shapeId;
        ShapeBounds = shapeBounds;
        EdgeBounds = edgeBounds;
        Reserved = reserved;
        UsesFillWindingRule = usesFillWindingRule;
        UsesNonScalingStrokes = usesNonScalingStrokes;
        UsesScalingStrokes = usesScalingStrokes;
        Shapes = shapes;
    }

    /// <summary>
    /// Read a DefineShape4 tag from the SWF reader.
    /// </summary>
    public static DefineShape4Tag Read(SwfReader reader)
    {
        int shapeId = reader.ReadUi16();
        var shapeBounds = Rectangle.Read(reader);
        var edgeBounds = Rectangle.Read(reader);

        var reserved = (int)reader.ReadUb(5);
        var usesFillWindingRule = reader.ReadBool();
        var usesNonScalingStrokes = reader.ReadBool();
        var usesScalingStrokes = reader.ReadBool();

        var shapes = ShapeWithStyle.Read(reader, version: 4);

        return new DefineShape4Tag(
            shapeId: shapeId,
            shapeBounds: shapeBounds,
            edgeBounds: edgeBounds,
            reserved: reserved,
            usesFillWindingRule: usesFillWindingRule,
            usesNonScalingStrokes: usesNonScalingStrokes,
            usesScalingStrokes: usesScalingStrokes,
            shapes: shapes
        );
    }
}